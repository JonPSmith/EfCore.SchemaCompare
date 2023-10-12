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
        var typeMapper = context.GetService<IRelationalTypeMappingSource>();

        object factoryObject;
        switch (providerName)
        {
            case SqliteProviderName:
                factoryObject = Activator.CreateInstance(factoryType, logger, typeMapper);
                break;
            case SqlServerProviderName:
                factoryObject = Activator.CreateInstance(factoryType, logger, typeMapper);
                break;
            case PostgresSqlProviderName:
                factoryObject = Activator.CreateInstance(factoryType, logger);
                break;
            default:
                // This is not a known provider. Try creating the factory anyhow and throw if it fails
                factoryObject = TryCreateUnknownFactory(factoryType, logger, providerName);
                break;
        }

        if (factoryObject is IDatabaseModelFactory factory)
            return factory;

        ThrowException(providerName);
        return (IDatabaseModelFactory) new object();
    }

    private static object TryCreateUnknownFactory(Type factoryType,
        IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger, string providerName)
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
                ThrowException(providerName);
            }
        }

        return null;
    }

    [DoesNotReturn]
    private static void ThrowException(string providerName)
    {
        throw new InvalidOperationException(
            $"It could not find the correct EF Core code to support the {providerName} database.");
    }
}