// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
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
            using var context = new OwnedTypeDbContext(options, OwnedTypeDbContext.Configs.NestedNull);

            //ATTEMPT
            var modelEntities = context.Model.GetEntityTypes().ToList();
            var userE = context.Entry(new User());
            var addressE = context.Entry(new Address());


            //VERIFY
            var m0 = modelEntities[0].GetProperties().ToList();
            var m1 = modelEntities[1].GetProperties().ToList();
            var m1n = modelEntities[1].GetDeclaredNavigations().ToList();

        }
    }
}