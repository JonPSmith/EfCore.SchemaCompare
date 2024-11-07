// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.ReadOnlyTypes.EfCode;
using EfSchemaCompare;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class TestComparerReadOnly
    {
        private readonly ITestOutputHelper _output;

        public TestComparerReadOnly(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CompareReadOnlyDbContextNotCheckedOk()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<ReadOnlyDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, ReadOnlyModelCacheKeyFactory>());
            using var context = new ReadOnlyDbContext(options);
            context.Database.EnsureClean();
            context.Database.ExecuteSqlRaw(
                "CREATE OR ALTER VIEW MyView AS SELECT Id, MyDateTime, MyString FROM NormalClasses");
            
            var comparer = new CompareEfSql();

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeTrue();
            var errors = CompareLog.ListAllErrors(comparer.Logs).ToList();
            (errors.Count == 1).ShouldBeTrue(comparer.GetAllErrors);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(
                "NOT CHECKED: Entity 'MappedToQuery', not mapped to database. Expected = <null>, found = MappedToQuery");
        }

        [Fact]
        public void CompareReadOnlyDbContextOk()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<ReadOnlyDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, ReadOnlyModelCacheKeyFactory>());
            using var context = new ReadOnlyDbContext(options);
            context.Database.EnsureClean();
            context.Database.ExecuteSqlRaw(
                "CREATE OR ALTER VIEW MyView AS SELECT Id, MyDateTime, MyString FROM NormalClasses");

            var config = new CompareEfSqlConfig();
            config.IgnoreTheseErrors(
                "NOT CHECKED: Entity 'MappedToQuery', not mapped to database. Expected = <null>, found = MappedToQuery");
            var comparer = new CompareEfSql(config);

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeFalse(comparer.GetAllErrors);
        }
        [Fact]
        public void CompareReadOnlyDbContextMissingView()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<ReadOnlyDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, ReadOnlyModelCacheKeyFactory>());
            using var context = new ReadOnlyDbContext(options);
            context.Database.EnsureClean();
            context.Database.ExecuteSqlRaw(
                "CREATE OR ALTER VIEW MyView AS SELECT Id, MyDateTime FROM NormalClasses");

            var config = new CompareEfSqlConfig();
            config.IgnoreTheseErrors(
                "NOT CHECKED: Entity 'MappedToQuery', not mapped to database. Expected = <null>, found = MappedToQuery");
            var comparer = new CompareEfSql(config);

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeTrue();
            var errors = CompareLog.ListAllErrors(comparer.Logs).ToList();
            (errors.Count == 1).ShouldBeTrue(comparer.GetAllErrors);
            errors[0].ShouldEqual(
                "NOT IN DATABASE: MappedToView->Property 'MyString', column name. Expected = <null>");
        }

        [Fact]
        public void CompareReadOnlyDbContextExtraViewColumn()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<ReadOnlyDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, ReadOnlyModelCacheKeyFactory>());
            using var context = new ReadOnlyDbContext(options);
            context.Database.EnsureClean();
            context.Database.ExecuteSqlRaw(
                "CREATE OR ALTER VIEW MyView AS SELECT Id, MyDateTime, MyInt, MyString FROM NormalClasses");

            var config = new CompareEfSqlConfig();
            config.IgnoreTheseErrors(
                "NOT CHECKED: Entity 'MappedToQuery', not mapped to database. Expected = <null>, found = MappedToQuery");
            var comparer = new CompareEfSql(config);

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeTrue();
            var errors = CompareLog.ListAllErrors(comparer.Logs).ToList();
            (errors.Count == 1).ShouldBeTrue(comparer.GetAllErrors);
            errors[0].ShouldEqual("EXTRA IN DATABASE: Column 'MyView', column name. Found = MyInt");
        }


        [Fact]
        public void CompareReadOnlyDbContextDifferentColumnType()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<ReadOnlyDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, ReadOnlyModelCacheKeyFactory>());
            using var context = new ReadOnlyDbContext(options, ReadOnlyDbContext.Configs.BadMappedToViewClass);
            context.Database.EnsureClean();
            context.Database.ExecuteSqlRaw(
                "CREATE OR ALTER VIEW MyView AS SELECT Id, MyDateTime, MyString FROM NormalClasses");

            var config = new CompareEfSqlConfig();
            config.IgnoreTheseErrors(
                "NOT CHECKED: Entity 'MappedToQuery', not mapped to database. Expected = <null>, found = MappedToQuery");
            var comparer = new CompareEfSql(config);

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeTrue();
            var errors = CompareLog.ListAllErrors(comparer.Logs).ToList();
            (errors.Count == 2).ShouldBeTrue(comparer.GetAllErrors);
            errors[0].ShouldEqual("DIFFERENT: MappedToViewBad->Property 'MyString', column type. Expected = int, found = nvarchar(max)");
            errors[1].ShouldEqual("DIFFERENT: MappedToViewBad->Property 'MyString', nullability. Expected = NOT NULL, found = NULL");
        }

        [Fact]
        public void CompareReadOnlyDbContextIgnoreNormalClasses()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<ReadOnlyDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, ReadOnlyModelCacheKeyFactory>());
            using var context = new ReadOnlyDbContext(options);
            context.Database.EnsureClean();
            var filepath = TestData.GetFilePath("AddViewToDatabase.sql");
            context.ExecuteScriptFileInTransaction(filepath);

            var config = new CompareEfSqlConfig
            {
                TablesToIgnoreCommaDelimited = "NormalClasses"
            };

            var comparer = new CompareEfSql(config);

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeTrue();
            var errors = CompareLog.ListAllErrors(comparer.Logs).ToList();
            errors.Count.ShouldEqual(3);
            errors[0].ShouldEqual(
                "NOT CHECKED: Entity 'MappedToQuery', not mapped to database. Expected = <null>, found = MappedToQuery");
            errors[1].ShouldEqual(
                "NOT IN DATABASE: MappedToQuery->Entity 'NormalClass', table name. Expected = NormalClasses");
            errors[2].ShouldEqual(
                "NOT IN DATABASE: MappedToQuery->Entity 'MappedToView', table name. Expected = NormalClasses");
        }

    }
}
