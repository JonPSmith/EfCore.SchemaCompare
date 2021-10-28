// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.OwnedTypes.EfCode;
using EfSchemaCompare;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class TestOwnedTypeOptions
    {
        private readonly ITestOutputHelper _output;

        public TestOwnedTypeOptions(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CompareOwnedTypeNestedNullable()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<OwnedTypeDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, OwnedTypeModelCacheKeyFactory>());
            using var context = new OwnedTypeDbContext(options, OwnedTypeDbContext.Configs.NestedNull);
            context.Database.EnsureClean();

            var comparer = new CompareEfSql();

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeFalse(comparer.GetAllErrors);
        }

        [Fact]
        public void CompareOwnedTypeNestedNotNullable()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<OwnedTypeDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, OwnedTypeModelCacheKeyFactory>());
            using var context = new OwnedTypeDbContext(options, OwnedTypeDbContext.Configs.NestedNotNull);
            context.Database.EnsureClean();

            var comparer = new CompareEfSql();

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeFalse(comparer.GetAllErrors);
        }

        [Fact]
        public void CompareOwnedTypeSeparateTable()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<OwnedTypeDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, OwnedTypeModelCacheKeyFactory>());
            using var context = new OwnedTypeDbContext(options, OwnedTypeDbContext.Configs.SeparateTable);
            context.Database.EnsureClean();

            var comparer = new CompareEfSql();

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeFalse(comparer.GetAllErrors);
        }

        [Fact]
        public void TestContextModelGetRelationalModelOk()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<OwnedTypeDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, OwnedTypeModelCacheKeyFactory>());
            using var contextNNull = new OwnedTypeDbContext(options, OwnedTypeDbContext.Configs.NestedNull);
            using var contextNNot = new OwnedTypeDbContext(options, OwnedTypeDbContext.Configs.NestedNotNull);

            //ATTEMPT 
            var relationalNNull = contextNNull.Model.GetRelationalModel().Tables.Single();
            var relationalNNot = contextNNot.Model.GetRelationalModel().Tables.Single();

            //VERIFY
            var comparer = new CompareHierarchical();
            comparer.CompareTwoSimilarClasses(relationalNNull, relationalNNot);
            foreach (var log in comparer.LoggedDiffs.Where(x => x.Status != CompareStatuses.CouldNotCompare))
            {
                _output.WriteLine(log.ToString());
            }
        }

        [Fact]
        public void TestContextModelGetRelationalModelColumnsOk()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<OwnedTypeDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, OwnedTypeModelCacheKeyFactory>());
            using var contextNNull = new OwnedTypeDbContext(options, OwnedTypeDbContext.Configs.NestedNull);
            using var contextNNot = new OwnedTypeDbContext(options, OwnedTypeDbContext.Configs.NestedNotNull);

            //ATTEMPT 
            var relationalNNull = contextNNull.Model.GetRelationalModel().Tables.Single().Columns.ToList();
            var relationalNNot = contextNNot.Model.GetRelationalModel().Tables.Single().Columns.ToList();

            //VERIFY

        }

        [Fact]
        public void TestGetModelOutputOk()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<OwnedTypeDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, OwnedTypeModelCacheKeyFactory>());
            using var context = new OwnedTypeDbContext(options, OwnedTypeDbContext.Configs.NestedNull);

            //ATTEMPT 
            var modelString = context.Model.ToDebugString(MetadataDebugStringOptions.LongDefault);

            //VERIFY
            _output.WriteLine(modelString);
        }

        [Fact]
        public void TestGetRelationalModelOutputOk()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<OwnedTypeDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, OwnedTypeModelCacheKeyFactory>());
            using var context = new OwnedTypeDbContext(options, OwnedTypeDbContext.Configs.NestedNull);

            //ATTEMPT
            var modelString = context.GetService<IDesignTimeModel>().Model.ToDebugString(MetadataDebugStringOptions.LongDefault);

            //VERIFY
            _output.WriteLine(modelString);
        }

    }
}