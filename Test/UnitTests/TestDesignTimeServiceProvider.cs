// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.BookApp.EfCode;
using EfSchemaCompare.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

#pragma warning disable EF1001 // Internal EF Core API usage.
namespace Test.UnitTests
{
    public class TestDesignTimeServiceProvider
    {
        private readonly ITestOutputHelper _output;

        public TestDesignTimeServiceProvider(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GetDatabaseProviderSqlServer()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                //ATTEMPT 
                var providerName = context.Database.ProviderName;

                //VERIFY
                providerName.ShouldEqual("Microsoft.EntityFrameworkCore.SqlServer");
            }
        }

        [Fact]
        public void GetDatabaseProviderSqlite()
        {
            //SETUP
            var optionsBuilder = SqliteInMemory.CreateOptions<BookContext>();
            using (var context = new BookContext(optionsBuilder))
            {
                //ATTEMPT 
                var providerName = context.Database.ProviderName;

                //VERIFY
                providerName.ShouldEqual("Microsoft.EntityFrameworkCore.Sqlite");
            }
        }

        [Fact]
        public void GetDatabaseProviderPostgres()
        {
            //SETUP
            var options = this.CreatePostgreSqlUniqueClassOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                //ATTEMPT 
                var providerName = context.Database.ProviderName;

                //VERIFY
                providerName.ShouldEqual("Npgsql.EntityFrameworkCore.PostgreSQL");
            }
        }

        [Fact]
        public void GetDatabaseModelFactorySqlServer()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                //ATTEMPT 
                var service = context.GetDatabaseModelFactory();

                //VERIFY
                service.ShouldNotBeNull();
                service.ShouldBeType<SqlServerDatabaseModelFactory>();
            }
        }

        [Fact]
        public void GetDatabaseModelFactorySqlite()
        {
            //SETUP
            var options = SqliteInMemory
                .CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                //ATTEMPT 
                var service = context.GetDatabaseModelFactory();

                //VERIFY
                service.ShouldNotBeNull();
                service.ShouldBeType<SqliteDatabaseModelFactory>();
            }
        }

        [Fact]
        public void GetDatabaseModelFactoryPostgres()
        {
            //SETUP
            var options = this.CreatePostgreSqlUniqueClassOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                //ATTEMPT 
                var service = context.GetDatabaseModelFactory();

                //VERIFY
                service.ShouldNotBeNull();
                service.ShouldBeType<NpgsqlDatabaseModelFactory>();
            }
        }

    }
}
