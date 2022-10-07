﻿// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable EF1001 // Internal EF Core API usage.
namespace EfSchemaCompare.Internal
{
    /// <summary>
    /// This static class contains the methods to return a design-time service provider
    /// </summary>
    public static class DesignProvider
    {
        private const string SqlServerProviderName = "Microsoft.EntityFrameworkCore.SqlServer";
        private const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";

        /// <summary>
        /// This returns the correct instance of the design time service for the current DbContext
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IDesignTimeServices GetDesignTimeService(this DbContext context)
        {
            var dbProvider = context.GetService<IDatabaseProvider>();
            if (dbProvider == null)
                throw new InvalidOperationException("Could not find a database provider service.");

            var providerName = dbProvider.Name;

            if (providerName == SqlServerProviderName)
                return new SqlServerDesignTimeServices();
            if (providerName == SqliteProviderName)
                return new SqliteDesignTimeServices();

            throw new InvalidOperationException("Your database provider isn't built into EfCore.SchemaCompare. This means you need to\n"+
                                                "use the CompareEfWithDb<T> method which takes in a design time class for your database. e.g.\n" +
                                                "comparer.CompareEfWithDb<NpgsqlDesignTimeServices>(context) for a Npgsql PostgreSql database provider.\n" +
                                                "Also look at the documentation in the EfCore.SchemaCompare README file");
        }

        /// <summary>
        /// This returns a DesignTimeProvider for the design time service instance that you provided
        /// </summary>
        /// <param name="designTimeService">This should be an instance of rhe design time service for the database provider</param>
        /// <returns></returns>
        public static ServiceProvider GetDesignTimeProvider(this IDesignTimeServices designTimeService)
        {
            // Add base services for scaffolding
            var serviceCollection = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices()
                .AddSingleton<IOperationReporter, OperationReporter>()
                .AddSingleton<IOperationReportHandler, OperationReportHandler>();
            
            designTimeService.ConfigureDesignTimeServices(serviceCollection);
            return serviceCollection.BuildServiceProvider();
        }
    }
}