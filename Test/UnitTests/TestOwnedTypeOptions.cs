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


        [Fact]
        public void LookAtEntityData()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<OwnedTypeDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, OwnedTypeModelCacheKeyFactory>());
            using var context1 = new OwnedTypeDbContext(options, OwnedTypeDbContext.Configs.NestedNull);
            using var context2 = new OwnedTypeDbContext(options, OwnedTypeDbContext.Configs.NestedNotNull);

            //ATTEMPT
            var user1E = context1.Entry(new User());
            var user2E = context2.Entry(new User());
            var address1E = context1.Entry(new Address());
            var address2E = context2.Entry(new Address());

            //VERIFY
            var a1Keys = address1E.Metadata.GetKeys().ToList();
            var a2Keys = address2E.Metadata.GetKeys().ToList();
            var u1Nav = user1E.Metadata.GetNavigations().ToList();
            var u2Nav = user2E.Metadata.GetNavigations().ToList();
        }
    }
}