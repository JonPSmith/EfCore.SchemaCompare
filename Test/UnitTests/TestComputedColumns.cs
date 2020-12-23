// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.MyEntityDb;
using EfSchemaCompare;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class TestComputedColumns
    {
        [Fact]
        public void CompareComputedColumn()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<MyEntityDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
            using var context = new MyEntityDbContext(options, MyEntityDbContext.Configs.ComputedCol);
            context.Database.EnsureClean();

            var comparer = new CompareEfSql();

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeFalse(comparer.GetAllErrors);
        }

        [Fact]
        public void ComparePersistentComputedColumn()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<MyEntityDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
            using var context = new MyEntityDbContext(options, MyEntityDbContext.Configs.PersistentComputedColumn);
            context.Database.EnsureClean();

            var comparer = new CompareEfSql();

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeFalse(comparer.GetAllErrors);
        }
    }
}