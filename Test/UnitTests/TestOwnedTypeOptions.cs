// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.OwnedTypes.EfClasses;
using DataLayer.OwnedTypes.EfCode;
using DataLayer.SpecialisedEntities.EfClasses;
using DataLayer.SpecialisedEntities.EfCode;
using EfSchemaCompare;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class TestOwnedTypeOptions
    {
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

    }
}