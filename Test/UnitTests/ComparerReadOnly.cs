// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.ReadOnlyTypes.EfCode;
using DataLayer.SpecialisedEntities.EfCode;
using EfSchemaCompare;
using Microsoft.EntityFrameworkCore;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class ComparerReadOnly
    {
        private readonly ITestOutputHelper _output;

        public ComparerReadOnly(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CompareReadOnlyDbContext()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<ReadOnlyDbContext>();
            using var context = new ReadOnlyDbContext(options);
            context.Database.EnsureClean();
            var filepath = TestData.GetFilePath("AddViewToDatabase.sql");
            context.ExecuteScriptFileInTransaction(filepath);
            
            var comparer = new CompareEfSql();

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeTrue();
            var errors = CompareLog.ListAllErrors(comparer.Logs).ToList();
            errors.Count.ShouldEqual(2);
            errors[0].ShouldEqual(
                "NOT CHECKED: Entity 'MappedToQuery', not mapped to database. Expected = <null>, found = MappedToQuery");
            errors[1].ShouldEqual(
                "NOT IN DATABASE: MappedToQuery->Entity 'ReadOnlyClass', table name. Expected = ReadOnlyClass");
        }

        [Fact]
        public void TestReadOnlyDbContext()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<ReadOnlyDbContext>();
            using var context = new ReadOnlyDbContext(options);
            context.Database.EnsureClean();
            
            

            //ATTEMPT
            var entities = context.Model.GetEntityTypes()
                .Select(x => new {x, TableName = x.GetTableName()})
                .ToList();

            //VERIFY

        }

    }
}
