// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

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
using Newtonsoft.Json.Converters;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

#pragma warning disable EF1001 // Internal EF Core API usage.
namespace Test.UnitTests
{
    public class Stage1ComparerMyEntityDiff
    {
        private readonly DatabaseModel databaseModel;
        private readonly ITestOutputHelper _output;

        public Stage1ComparerMyEntityDiff(ITestOutputHelper output)
        {
            _output = output;

            var serviceProvider = new SqlServerDesignTimeServices().GetDesignTimeProvider();
            var factory = serviceProvider.GetService<IDatabaseModelFactory>();

            var options = this.CreateUniqueClassOptions<MyEntityDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
            using (var context = new MyEntityDbContext(options, MyEntityDbContext.Configs.NormalTable))
            {
                context.Database.EnsureClean();
                var connectionString = context.Database.GetDbConnection().ConnectionString;
                databaseModel = factory.Create(connectionString,
                    new DatabaseModelFactoryOptions(new string[] { }, new string[] { }));
            }
        }

        [Fact]
        public void CompareDefaultConfigNoErrors()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<MyEntityDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
            using (var context = new MyEntityDbContext(options, MyEntityDbContext.Configs.NormalTable))
            {
                var model = context.Model;
                var handler = new Stage1Comparer(model, context.GetType().Name);

                //ATTEMPT
                var hasErrors = handler.CompareModelToDatabase(databaseModel);

                //VERIFY
                hasErrors.ShouldBeFalse();
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter());
                var json = JsonConvert.SerializeObject(handler.Logs, settings);
            }
        }

        [Fact]
        public void CompareTableSchemaConfig()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<MyEntityDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
            using (var context = new MyEntityDbContext(options, MyEntityDbContext.Configs.TableWithSchema))
            {
                var model = context.Model;
                var handler = new Stage1Comparer(model, context.GetType().Name);

                //ATTEMPT
                var hasErrors = handler.CompareModelToDatabase(databaseModel);

                //VERIFY
                hasErrors.ShouldBeTrue();
                CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                    "NOT IN DATABASE: Entity 'MyEntity', table name. Expected = MySchema.MyEntities");
            }
        }

        [Fact]
        public void CompareTotalSchemaConfig()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<MyEntityDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
            using (var context = new MyEntityDbContext(options, MyEntityDbContext.Configs.WholeSchemaSet))
            {
                var model = context.Model;
                var handler = new Stage1Comparer(model, context.GetType().Name);

                //ATTEMPT
                var hasErrors = handler.CompareModelToDatabase(databaseModel);

                //VERIFY
                hasErrors.ShouldBeTrue();
                CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                    "NOT IN DATABASE: Entity 'MyEntity', table name. Expected = AllSchema.MyEntites");
            }
        }

        [Fact]
        public void CompareShadowProperty()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<MyEntityDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
            using (var context = new MyEntityDbContext(options, MyEntityDbContext.Configs.ShadowProp))
            {
                var model = context.Model;
                var handler = new Stage1Comparer(model, context.GetType().Name);

                //ATTEMPT
                var hasErrors = handler.CompareModelToDatabase(databaseModel);

                //VERIFY
                hasErrors.ShouldBeTrue();
                CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                    "NOT IN DATABASE: MyEntity->Property 'ShadowProp', column name. Expected = ShadowProp");
            }
        }

        [Fact]
        public void ComparePropertyDiffColName()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<MyEntityDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
            using (var context = new MyEntityDbContext(options, MyEntityDbContext.Configs.DifferentColName))
            {
                var model = context.Model;
                var handler = new Stage1Comparer(model, context.GetType().Name);

                //ATTEMPT
                var hasErrors = handler.CompareModelToDatabase(databaseModel);

                //VERIFY
                hasErrors.ShouldBeTrue();
                CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                    "NOT IN DATABASE: MyEntity->Property 'MyInt', column name. Expected = OtherColName");
            }
        }

        [Fact]
        public void ComparePropertyDiffNullName()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<MyEntityDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
            using (var context = new MyEntityDbContext(options, MyEntityDbContext.Configs.StringIsRequired))
            {
                var model = context.Model;
                var handler = new Stage1Comparer(model, context.GetType().Name);

                //ATTEMPT
                var hasErrors = handler.CompareModelToDatabase(databaseModel);

                //VERIFY
                hasErrors.ShouldBeTrue();
                CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyString', nullability. Expected = NOT NULL, found = NULL");
            }
        }

        [Fact]
        public void ComparePropertyDiffTypeName()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<MyEntityDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
            using (var context = new MyEntityDbContext(options, MyEntityDbContext.Configs.StringIsAscii))
            {
                var model = context.Model;
                var handler = new Stage1Comparer(model, context.GetType().Name);

                //ATTEMPT
                var hasErrors = handler.CompareModelToDatabase(databaseModel);

                //VERIFY
                hasErrors.ShouldBeTrue();
                CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyString', column type. Expected = varchar(max), found = nvarchar(max)");
            }
        }

        [Fact]
        public void CompareDiffPrimaryKey()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<MyEntityDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
            using (var context = new MyEntityDbContext(options, MyEntityDbContext.Configs.DifferentPk))
            {
                var model = context.Model;
                var handler = new Stage1Comparer(model, context.GetType().Name);

                //ATTEMPT
                var hasErrors = handler.CompareModelToDatabase(databaseModel);

                //VERIFY
                hasErrors.ShouldBeTrue();
                var errors = CompareLog.ListAllErrors(handler.Logs).ToList();
                errors.Count.ShouldEqual(4);
                errors[0].ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyInt', value generated. Expected = OnAdd, found = Never");
                errors[1].ShouldEqual(
                    "NOT IN DATABASE: MyEntity->PrimaryKey 'PK_MyEntites', column name. Expected = MyInt");
                errors[2].ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyEntityId', value generated. Expected = Never, found = OnAdd");
                errors[3].ShouldEqual(
                    "EXTRA IN DATABASE: MyEntity->PrimaryKey 'PK_MyEntites', column name. Found = MyEntityId");
            }
        }

        [Fact]
        public void CompareDiffIndexes()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<MyEntityDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
            using (var context = new MyEntityDbContext(options, MyEntityDbContext.Configs.HasUniqueIndex))
            {
                var model = context.Model;
                var handler = new Stage1Comparer(model, context.GetType().Name);

                //ATTEMPT
                var hasErrors = handler.CompareModelToDatabase(databaseModel);

                //VERIFY
                hasErrors.ShouldBeTrue();
                var errors = CompareLog.ListAllErrors(handler.Logs).ToList();
                errors.Count.ShouldEqual(3);
                errors[0].ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyString', column type. Expected = nvarchar(450), found = nvarchar(max)");
                errors[1].ShouldEqual(
                    "NOT IN DATABASE: MyEntity->Index 'MyInt', index constraint name. Expected = MySpecialName");
                errors[2].ShouldEqual(
                    "NOT IN DATABASE: MyEntity->Index 'MyString', index constraint name. Expected = IX_MyEntites_MyString");
            }
        }

        [Fact]
        public void ComparePropertyComputedColName()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<MyEntityDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
            using (var context = new MyEntityDbContext(options, MyEntityDbContext.Configs.ComputedCol))
            {
                var model = context.Model;
                var handler = new Stage1Comparer(model, context.GetType().Name);

                //ATTEMPT
                var hasErrors = handler.CompareModelToDatabase(databaseModel);

                //VERIFY
                hasErrors.ShouldBeTrue();
                var errors = CompareLog.ListAllErrors(handler.Logs).ToList();
                errors.Count.ShouldEqual(2);
                errors[0].ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyDateTime', computed column sql. Expected = getutcdate(), found = <null>");
                errors[1].ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyDateTime', value generated. Expected = OnAddOrUpdate, found = Never");
            }
        }

        [Fact]
        public void ComparePropertyComputedColSelf()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<MyEntityDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
            using (var context = new MyEntityDbContext(options, MyEntityDbContext.Configs.ComputedCol))
            {
                var model = context.Model;
                var handler = new Stage1Comparer(model, context.GetType().Name);

                //ATTEMPT
                var hasErrors = handler.CompareModelToDatabase(databaseModel);

                //VERIFY
                hasErrors.ShouldBeTrue();
                //The setting of a computed col changed the column type
                var errors = CompareLog.ListAllErrors(handler.Logs).ToList();
                errors.Count.ShouldEqual(2);
                errors[1].ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyDateTime', value generated. Expected = OnAddOrUpdate, found = Never");

            }
        }

        [Fact]
        public void ComparePropertySqlDefaultName()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<MyEntityDbContext>(
                builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
            using (var context = new MyEntityDbContext(options, MyEntityDbContext.Configs.DefaultValue))
            {
                var model = context.Model;
                var handler = new Stage1Comparer(model, context.GetType().Name);

                //ATTEMPT
                var hasErrors = handler.CompareModelToDatabase(databaseModel);

                //VERIFY
                hasErrors.ShouldBeTrue();
                var errors = CompareLog.ListAllErrors(handler.Logs).ToList();
                errors.Count.ShouldEqual(4);
                errors[0].ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyDateTime', default value sql. Expected = 2000-01-01T00:00:00, found = <null>");
                errors[1].ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyDateTime', value generated. Expected = OnAdd, found = Never");
                errors[2].ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyString', default value sql. Expected = Hello!, found = <null>");
                errors[3].ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyString', value generated. Expected = OnAdd, found = Never");
            }
        }

    }
}
