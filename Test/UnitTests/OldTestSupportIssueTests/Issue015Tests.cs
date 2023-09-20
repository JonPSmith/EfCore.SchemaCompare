// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.OldTestSupportDbs.Issue015;
using EfSchemaCompare;
using Microsoft.EntityFrameworkCore;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.OldTestSupportIssueTests
{
    public class Issue015Tests
    {
        private readonly string _connectionString;
        private readonly DbContextOptions<Issue15DbContext> _options;
        private readonly ITestOutputHelper _output;

        public Issue015Tests(ITestOutputHelper output)
        {
            _output = output;
            _options = this
                .CreateUniqueClassOptions<Issue15DbContext>();

            using (var context = new Issue15DbContext(_options))
            {
                _connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureClean();
            }
        }

        [Fact]
        public void TestDifferentDefaultValuesReturnErrors()
        {
            //SETUP
            using (var context = new Issue15DbContext(_options))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(context);

                //VERIFY
                hasErrors.ShouldBeTrue();

                //This changed with EF Core 8. Now HasDefaultValueSql of Enums and int now set the expected 
                comparer.GetAllErrors.ShouldEqual(
@"DIFFERENT: Message->Property 'BoolRequiredDefaultFalse', default value sql. Expected = CAST(0 AS bit), found = CONVERT([bit],(0))
DIFFERENT: Message->Property 'BoolRequiredDefaultTrue', default value sql. Expected = CAST(1 AS bit), found = CONVERT([bit],(1))
DIFFERENT: Message->Property 'XmlRequiredDefaultEmpty', default value sql. Expected = N'', found = ''
DIFFERENT: Message->Property 'XmlRequiredDefaultSomething', default value sql. Expected = N'<something />', found = '<something />'");
            }
        }

        [Fact]
        public void TestDifferentDefaultValuesSuppressDefaultValueErrors()
        {
            //SETUP
            using (var context = new Issue15DbContext(_options))
            {
                var config = new CompareEfSqlConfig();
                config.AddIgnoreCompareLog(new CompareLog(CompareType.Property, CompareState.Different, null, CompareAttributes.DefaultValueSql));
                config.AddIgnoreCompareLog(new CompareLog(CompareType.Property, CompareState.Different, null, CompareAttributes.ValueGenerated, "OnAdd", "Never"));
                var comparer = new CompareEfSql(config);

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(context);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }
    }
}