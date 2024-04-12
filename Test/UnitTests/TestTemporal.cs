// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.MyEntityDb;
using EfSchemaCompare;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class TestTemporal
    {
        [Fact]
        public void CompareTemporal()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<MyEntityDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
            using var context = new MyEntityDbContext(options, MyEntityDbContext.Configs.Temporal);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            foreach (var entityType in context.GetService<IDesignTimeModel>().Model.GetEntityTypes())
                context.Database.ExecuteSqlRaw(
                    $"IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{entityType.GetTableName()}') ALTER TABLE [{entityType.GetTableName()}] SET (SYSTEM_VERSIONING = OFF)");
            context.Database.EnsureClean();

            var comparer = new CompareEfSql();

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeFalse(comparer.GetAllErrors);
        }
    }
}