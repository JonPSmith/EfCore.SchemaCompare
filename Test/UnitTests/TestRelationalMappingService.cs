// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using DataLayer.SpecialisedEntities.EfCode;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class TestRelationalMappingService
    {
        private enum Settings { Zero, One, Two}
        
        
        [Fact]
        public void TestRelationalTypeMappingSourceOk()
        {

            //SETUP
            var options = this.CreateUniqueClassOptions<SpecializedDbContext>();
            using var context = new SpecializedDbContext(options);
            context.Database.EnsureClean();

            var typeMappingSource = context.GetService<IRelationalTypeMappingSource>();
            
            //ATTEMPT 
            var constantString = typeMappingSource.FindMapping(typeof(string)).GenerateSqlLiteral("hello");
            var constantInt = typeMappingSource.FindMapping(typeof(int)).GenerateSqlLiteral(123);
            var constantEnum = typeMappingSource.FindMapping(typeof(Settings)).GenerateSqlLiteral(Settings.One);
            var constantBool = typeMappingSource.FindMapping(typeof(bool)).GenerateSqlLiteral(true);

            //VERIFY
            constantString.ShouldEqual("N'hello'");
            constantInt.ShouldEqual("123");
            constantEnum.ShouldEqual("1");
            constantBool.ShouldEqual("CAST(1 AS bit)");
        }
    }
}