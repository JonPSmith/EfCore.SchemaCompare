// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EfSchemaCompare.Internal;

internal static class DatabaseModelFinder
{
    private const string SqlServerProviderName = "Microsoft.EntityFrameworkCore.SqlServer";
    private const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";
    private const string PostgresSqlProviderName = "Npgsql.EntityFrameworkCore.PostgreSQL";

    public static IDatabaseModelFactory GetDatabaseModelFactory(this DbContext context)
    {
        var providerName = context.Database.ProviderName;

        var logger = context.GetService<IDiagnosticsLogger<DbLoggerCategory.Scaffolding>>();

        var providerAssembly = Assembly.Load(providerName!);
        var factoryType = providerAssembly.ExportedTypes.First(x => x.BaseType == typeof(DatabaseModelFactory));
        
        object factoryObject;
        switch (providerName)
        {
            case SqliteProviderName:
                var typeMapper = context.GetService<IRelationalTypeMappingSource>();
                factoryObject = Activator.CreateInstance(factoryType, logger, typeMapper);
                break;
            case SqlServerProviderName:
            case PostgresSqlProviderName:
                factoryObject = Activator.CreateInstance(factoryType, logger);
                break;
            default:
                // This is not a known provider. Try creating the factory anyhow and throw if it fails
                factoryObject = TryCreateUnknownFactory(factoryType, logger);
                break;
        }

        if (factoryObject is IDatabaseModelFactory factory)
            return factory;

        ThrowException();
        return (IDatabaseModelFactory) new object();
    }

    private static object TryCreateUnknownFactory(Type factoryType,
        IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
    {
        try
        {
            return Activator.CreateInstance(factoryType);
        }
        catch
        {
            try
            {
                return Activator.CreateInstance(factoryType, logger);
            }
            catch
            {
                ThrowException();
            }
        }

        return null;
    }

    [DoesNotReturn]
    private static void ThrowException()
    {
        throw new InvalidOperationException(
            "Your database provider isn't supported by the EfCore.SchemaCompare library. "
            + "Please provide an issue about the database type you would like and I may be able to add it.");
    }
}