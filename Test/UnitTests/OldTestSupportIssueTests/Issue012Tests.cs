// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.OldTestSupportDbs.Issue012;
using EfSchemaCompare;
using EfSchemaCompare.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.DependencyInjection;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.OldTestSupportIssueTests
{
    public class Issue012Tests
    {
        private readonly string _connectionString;
        private readonly DbContextOptions<Issue012DbContext> _options;
        private readonly ITestOutputHelper _output;

        public Issue012Tests(ITestOutputHelper output)
        {
            _output = output;
            _options = this
                .CreateUniqueClassOptions<Issue012DbContext>();

            using (var context = new Issue012DbContext(_options))
            {
                _connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureClean();
            }
        }

        [Fact]
        public void TestTwoTablesWithSameNameButDifferentSchemas()
        {
            //SETUP
            using (var context = new Issue012DbContext(_options))
            {
                var factory = context.GetDatabaseModelFactory();
                var database = factory.Create(_connectionString,
                    new DatabaseModelFactoryOptions(new string[] { }, new string[] { }));

                var handler = new Stage1Comparer(context);

                //ATTEMPT
                var hasErrors = handler.CompareModelToDatabase(database);

                //VERIFY
                foreach (var log in CompareLog.AllResultsIndented(handler.Logs))
                {
                    _output.WriteLine(log);
                }
                hasErrors.ShouldBeFalse();
            }
        }
    }
}