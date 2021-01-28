// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.SpecialisedEntities.EfCode;
using EfSchemaCompare;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class ComparerSpecialized
    {
        private readonly ITestOutputHelper _output;

        public ComparerSpecialized(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CompareSpecializedDbContext()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<SpecializedDbContext>();
            using var context = new SpecializedDbContext(options);
            context.Database.EnsureClean();

            var comparer = new CompareEfSql();

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeFalse(comparer.GetAllErrors);
        }

    }
}
