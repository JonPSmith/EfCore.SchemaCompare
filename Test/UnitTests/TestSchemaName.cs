// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.MyEntityDb;
using EfSchemaCompare.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.DependencyInjection;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class TestSchemaName
    {
        private readonly ITestOutputHelper _output;

        public TestSchemaName(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData(MyEntityDbContext.Configs.NormalTable)]
        [InlineData(MyEntityDbContext.Configs.WholeSchemaSet)]
        public void TestSchemaDefaultNameSqlServer(MyEntityDbContext.Configs config)
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<MyEntityDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
            using var context = new MyEntityDbContext(options, config);
            context.Database.EnsureClean();

            var factory = context.GetDatabaseModelFactory();

            //ATTEMPT
            var database = factory.Create(context.Database.GetConnectionString(),
                new DatabaseModelFactoryOptions(new string[] { }, new string[] { }));

            //VERIFY
            database.DefaultSchema.ShouldEqual("dbo");
        }

        [Theory]
        [InlineData(MyEntityDbContext.Configs.NormalTable)]
        [InlineData(MyEntityDbContext.Configs.WholeSchemaSet)]
        public void TestSchemaDefaultNameSqlite(MyEntityDbContext.Configs config)
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<MyEntityDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
            using var context = new MyEntityDbContext(options, config);
            context.Database.EnsureCreated();

            var factory = context.GetDatabaseModelFactory();

            //ATTEMPT
            var database = factory.Create(context.Database.GetConnectionString(),
                new DatabaseModelFactoryOptions(new string[] { }, new string[] { }));

            //VERIFY
            database.DefaultSchema.ShouldEqual(null);
        }


    }
}
