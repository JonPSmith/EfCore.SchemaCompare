// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.BookApp.EfCode;
using EfSchemaCompare.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

#pragma warning disable EF1001 // Internal EF Core API usage.
namespace Test.UnitTests
{
    public class TestDesignTimeServiceProvider
    {
        private readonly string _connectionString;
        private readonly DbContextOptions<BookContext> _options;

        private readonly ITestOutputHelper _output;

        public TestDesignTimeServiceProvider(ITestOutputHelper output)
        {
            _output = output;
            _options = this
                .CreateUniqueClassOptions<BookContext>();

            using (var context = new BookContext(_options))
            {
                _connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureCreated();
            }
        }

        [Fact]
        public void GetDatabaseProviderSqlServer()
        {
            //SETUP
            using (var context = new BookContext(_options))
            {
                //ATTEMPT 
                var dbProvider = context.GetService<IDatabaseProvider>();

                //VERIFY
                dbProvider.ShouldNotBeNull();
                dbProvider.Name.ShouldEqual("Microsoft.EntityFrameworkCore.SqlServer");
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
                var dbProvider = context.GetService<IDatabaseProvider>();

                //VERIFY
                dbProvider.ShouldNotBeNull();
                dbProvider.Name.ShouldEqual("Microsoft.EntityFrameworkCore.Sqlite");
            }
        }

        [Fact]
        public void GetDesignTimeServiceProviderSqlServer()
        {
            //SETUP
            using (var context = new BookContext(_options))
            {
                //ATTEMPT 
                var service = context.GetDesignTimeService();

                //VERIFY
                service.ShouldNotBeNull();
                service.ShouldBeType<SqlServerDesignTimeServices>();
            }
        }

        [Fact]
        public void GetDesignTimeServiceProviderSqlite()
        {
            //SETUP
            var options = SqliteInMemory
                .CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                //ATTEMPT 
                var service = context.GetDesignTimeService();

                //VERIFY
                service.ShouldNotBeNull();
                service.ShouldBeType<SqliteDesignTimeServices>();
            }
        }

        [Fact]
        public void GetIDatabaseModelFactorySqlServer()
        {
            //SETUP
            using (var context = new BookContext(_options))
            {
                var dtService = context.GetDesignTimeService();
                var serviceProvider = dtService.GetDesignTimeProvider();

                //ATTEMPT 
                var factory = serviceProvider.GetService<IDatabaseModelFactory>();

                //VERIFY
                factory.ShouldNotBeNull();
                factory.ShouldBeType<SqlServerDatabaseModelFactory>();
            }
        }

        [Fact]
        public void GetIDatabaseModelFactorySqlite()
        {
            //SETUP
            var options = SqliteInMemory
                .CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                var dtService = context.GetDesignTimeService();
                var serviceProvider = dtService.GetDesignTimeProvider();

                //ATTEMPT 
                var factory = serviceProvider.GetService<IDatabaseModelFactory>();

                //VERIFY
                factory.ShouldNotBeNull();
                factory.ShouldBeType<SqliteDatabaseModelFactory>();
            }
        }

        [Fact]
        public void GetIDatabaseModelFactoryDatabaseModel()
        {
            //SETUP
            using (var context = new BookContext(_options))
            {
                var dtService = context.GetDesignTimeService();
                var serviceProvider = dtService.GetDesignTimeProvider();
                var factory = serviceProvider.GetService<IDatabaseModelFactory>();

                //ATTEMPT

                var databaseModel = factory.Create(context.Database.GetConnectionString(),
                    new DatabaseModelFactoryOptions(new string[] { }, new string[] { }));

                //VERIFY
                foreach (var databaseModelTable in databaseModel.Tables)
                {
                    _output.WriteLine(databaseModelTable.Name);
                }
            }
        }

        [Fact]
        public void GetIScaffoldingModelFactory()
        {
            //SETUP
            var serviceProvider = new SqlServerDesignTimeServices().GetDesignTimeProvider();

            //ATTEMPT 
            var factory = serviceProvider.GetService<IScaffoldingModelFactory>();

            //VERIFY
            factory.ShouldNotBeNull();
            factory.ShouldBeType<RelationalScaffoldingModelFactory>();
        }
    }
}
