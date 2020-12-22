// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.BookApp.EfCode;
using EfSchemaCompare;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class ComparerBooksMtoM
    {
        private readonly ITestOutputHelper _output;

        public ComparerBooksMtoM(ITestOutputHelper output)
        {
            _output = output;

        }

        [Fact]
        public void TestMtoMUsingDictionary()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<BookContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, BookContextModelCacheKeyFactory>());

            using var context = new BookContext(options, BookContext.Configs.M2MDict);
            context.Database.EnsureClean();
            
            var comparer = new CompareEfSql();

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeFalse(comparer.GetAllErrors);
        }

        [Fact]
        public void TestMtoMUsingClass()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<BookContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, BookContextModelCacheKeyFactory>());
            using var context = new BookContext(options, BookContext.Configs.M2MProvided);
            context.Database.EnsureClean();

            var comparer = new CompareEfSql();

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeFalse(comparer.GetAllErrors);
        }
    }
}
