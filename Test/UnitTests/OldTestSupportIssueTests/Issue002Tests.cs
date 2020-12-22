// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.OldTestSupportDbs.Issue002;
using EfSchemaCompare;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.OldTestSupportIssueTests
{
    public class Issue002Tests
    {
        [Fact]
        public void CompareIssue2()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<Issue2DbContext>();
            using (var context = new Issue2DbContext(options))
            {
                //context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(context);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }
    }
}