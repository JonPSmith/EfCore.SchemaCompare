// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using DataLayer.MyEntityDb;
using EfSchemaCompare;
using EfSchemaCompare.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

#pragma warning disable EF1001 // Internal EF Core API usage.
namespace Test.UnitTests
{
    public class Stage2ComparerMyEntityDiff
    {
        private readonly DatabaseModel _databaseModel;
        private readonly ITestOutputHelper _output;

        public Stage2ComparerMyEntityDiff(ITestOutputHelper output)
        {
            _output = output;
            var options = this
                .CreateUniqueClassOptions<MyEntityDbContext>();
            var serviceProvider = new SqlServerDesignTimeServices().GetDesignTimeProvider();
            var factory = serviceProvider.GetService<IDatabaseModelFactory>();

            using (var context = new MyEntityDbContext(options, MyEntityDbContext.Configs.NormalTable))
            {
                var connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureClean();

                _databaseModel = factory.Create(connectionString,
                    new DatabaseModelFactoryOptions(new string[] { }, new string[] { }));
            }
        }

        [Fact]
        public void ExtrasNoErrors()
        {
            //SETUP
            var firstStageLogs =
                JsonConvert.DeserializeObject<List<CompareLog>>(TestData.GetFileContent("DbContextCompareLog01*.json"));
            var handler = new Stage2Comparer(_databaseModel);

            //ATTEMPT
            var hasErrors = handler.CompareLogsToDatabase(firstStageLogs);

            //VERIFY
            hasErrors.ShouldBeFalse();
        }

        [Fact]
        public void ExtrasTable()
        {
            //SETUP
            var jArray = JArray.Parse(TestData.GetFileContent("DbContextCompareLog01*.json"));
            jArray[0]["SubLogs"][0]["Expected"] = "DiffTableName";
            var firstStageLogs = JsonConvert.DeserializeObject<List<CompareLog>>(jArray.ToString());

            var handler = new Stage2Comparer(_databaseModel);

            //ATTEMPT
            var hasErrors = handler.CompareLogsToDatabase(firstStageLogs);

            //VERIFY
            hasErrors.ShouldBeTrue();
            CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                "EXTRA IN DATABASE: Table 'MyEntites'");
        }

        [Fact]
        public void ExtrasProperty()
        {
            //SETUP
            var jArray = JArray.Parse(TestData.GetFileContent("DbContextCompareLog01*.json"));
            jArray[0]["SubLogs"][0]["SubLogs"][0]["Expected"] = "DiffPropName";
            var firstStageLogs = JsonConvert.DeserializeObject<List<CompareLog>>(jArray.ToString());

            var handler = new Stage2Comparer(_databaseModel);

            //ATTEMPT
            var hasErrors = handler.CompareLogsToDatabase(firstStageLogs);

            //VERIFY
            hasErrors.ShouldBeTrue();
            CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                "EXTRA IN DATABASE: Column 'MyEntites', column name. Found = MyEntityId");
        }

        [Fact]
        public void ExtraIndexConstaint()
        {
            //SETUP
            var firstStageLogs = JsonConvert.DeserializeObject<List<CompareLog>>(
                TestData.GetFileContent("DbContextCompareLog01*.json"));

            var options = this.CreateUniqueClassOptions<MyEntityDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
            using (var context = new MyEntityDbContext(options, MyEntityDbContext.Configs.HasIndex))
            {
                var dtService = context.GetDesignTimeService();
                var serviceProvider = dtService.GetDesignTimeProvider();
                var factory = serviceProvider.GetService<IDatabaseModelFactory>();
                var connectionString = context.Database.GetDbConnection().ConnectionString;

                context.Database.EnsureClean();

                var databaseModel = factory.Create(connectionString,
                    new DatabaseModelFactoryOptions(new string[] { }, new string[] { }));

                var handler = new Stage2Comparer(databaseModel);

                //ATTEMPT
                var hasErrors = handler.CompareLogsToDatabase(firstStageLogs);

                //VERIFY
                hasErrors.ShouldBeTrue();
                CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                    "EXTRA IN DATABASE: Index 'MyEntites', index constraint name. Found = IX_MyEntites_MyInt");
            }
        }
    }
}
