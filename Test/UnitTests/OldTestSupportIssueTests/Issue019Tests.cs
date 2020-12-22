// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using DataLayer.OldTestSupportDbs.Issue019;
using EfSchemaCompare;
using Microsoft.EntityFrameworkCore;
using TestSupport.Helpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.OldTestSupportIssueTests
{
    public class Issue019Tests
    {
        private string _fullCaseConnectionString;
        private DbContextOptions<Issue19FullCaseDbContext> _fullCaseOptions;
        private string _lowerCaseConnectionString;
        private DbContextOptions<Issue19LowerCaseDbContext> _lowerCaseOptions;

        public Issue019Tests()
        {
            _fullCaseConnectionString = this.GetUniqueDatabaseConnectionString("FullCase");
            _lowerCaseConnectionString = this.GetUniqueDatabaseConnectionString("LowerCase");
            var fullOptionsBuilder = new DbContextOptionsBuilder<Issue19FullCaseDbContext>();
            fullOptionsBuilder.UseSqlServer(_fullCaseConnectionString);
            _fullCaseOptions = fullOptionsBuilder.Options;
            var lowerOptionsBuilder = new DbContextOptionsBuilder<Issue19LowerCaseDbContext>();
            lowerOptionsBuilder.UseSqlServer(_lowerCaseConnectionString);
            _lowerCaseOptions = lowerOptionsBuilder.Options;

            //Ensure the databases exist
            using (var context = new Issue19FullCaseDbContext(_fullCaseOptions))
            {
                context.Database.EnsureCreated();
            }
            using (var context = new Issue19LowerCaseDbContext(_lowerCaseOptions))
            {
                context.Database.EnsureCreated();
            }
        }

        [Fact]
        public void CompareFullCaseWithItself()
        {
            //SETUP
            using (var context = new Issue19FullCaseDbContext(_fullCaseOptions))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(context);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }

        [Fact]
        public void CompareLowerCaseWithItself()
        {
            //SETUP
            using (var context = new Issue19LowerCaseDbContext(_lowerCaseOptions))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(context);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }

        [Fact]
        public void CompareLowerCaseToUpperCaseDatabase()
        {
            //SETUP
            using (var context = new Issue19LowerCaseDbContext(_lowerCaseOptions))
            {
                var config = new CompareEfSqlConfig {CaseComparer = StringComparer.CurrentCultureIgnoreCase};
                var comparer = new CompareEfSql(config);

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(_fullCaseConnectionString, context);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }
    }
}