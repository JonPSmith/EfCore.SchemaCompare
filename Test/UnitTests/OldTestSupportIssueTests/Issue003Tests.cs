// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.OldTestSupportDbs.Issue003;
using EfSchemaCompare;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.OldTestSupportIssueTests
{
    public class Issue003Tests
    {
        [Fact]
        public void CompareIssue3()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<Issue3DbContext>();
            using (var context = new Issue3DbContext(options))
            {
                //context.Database.EnsureDeleted();
                context.Database.EnsureClean();
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(context);

                //VERIFY
                hasErrors.ShouldBeTrue(comparer.GetAllErrors);
                var errors = CompareLog.ListAllErrors(comparer.Logs).ToList();
                errors.Count.ShouldEqual(1);
                errors[0].ShouldEqual(
                    "DIFFERENT: Parameter->Property 'ValueAggregationTypeId', default value sql. Expected = CAST(1 AS tinyint), found = CONVERT([tinyint],(1))");
            }
        }
    }
}