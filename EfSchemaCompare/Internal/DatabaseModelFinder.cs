// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Sqlite.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal;

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

        if (providerName == SqlServerProviderName)
            return new SqlServerDatabaseModelFactory(logger);
        if (providerName == SqliteProviderName)
        {
            var typeMapper = context.GetService<IRelationalTypeMappingSource>();
            return new SqliteDatabaseModelFactory(logger, typeMapper);
        }
        if (providerName == PostgresSqlProviderName)
            return new NpgsqlDatabaseModelFactory(logger);

        throw new InvalidOperationException("Your database provider isn't supported by the EfCore.SchemaCompare library. " +
                                            "Please provide an issue about the database type you would like and I may be able to add it.");
    }
}