// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.MyEntityDb;
using DataLayer.ReadOnlyTypes.EfCode;
using EfSchemaCompare.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;
using System.Linq;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests;

public class TestDefaultSchemaNameViaScaffolding
{
    private readonly ITestOutputHelper _output;

    public TestDefaultSchemaNameViaScaffolding(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData(MyEntityDbContext.Configs.NormalTable)]
    [InlineData(MyEntityDbContext.Configs.TableWithSchema)]
    [InlineData(MyEntityDbContext.Configs.WholeSchemaSet)]
    public void TestSchemaDefaultNameSqlServer(MyEntityDbContext.Configs config)
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<MyEntityDbContext>(
            builder => builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>());
        using var context = new MyEntityDbContext(options, config);
        context.Database.EnsureClean();

        var factory = context.GetDatabaseModelFactory();

        //ATTEMPT
        var database = factory.Create(context.Database.GetConnectionString(),
            new DatabaseModelFactoryOptions(new string[] { }, new string[] { }));

        //VERIFY
        database.DefaultSchema.ShouldEqual("dbo");

        var entity = context.Model.GetEntityTypes().First();
        _output.WriteLine(config.ToString());
        _output.WriteLine($"   context.Model entity Name              = {entity.Name}");
        _output.WriteLine($"   context.Model FormSchemaTableFromModel = {entity.FormSchemaTableFromModel()}");
        _output.WriteLine($"   Scaffold.Model entity name             = {database.Tables.First().Name}");
        _output.WriteLine($"   Scaffold.Model entity schema           = {database.Tables.First().Schema}");
    }

    [Theory]
    [InlineData(MyEntityDbContext.Configs.NormalTable)]
    [InlineData(MyEntityDbContext.Configs.WholeSchemaSet)]
    public void TestSchemaDefaultNameSqlite(MyEntityDbContext.Configs config)
    {
        //SETUP
        var builder  = new DbContextOptionsBuilder<MyEntityDbContext>()
            .UseSqlite($"Data Source={TestData.GetTestDataDir()}/TestSchemaDefaultNameSqlite.db")
            .ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>();
        using var context = new MyEntityDbContext(builder.Options, config);
        context.Database.EnsureCreated();

        var factory = context.GetDatabaseModelFactory();

        //ATTEMPT
        var database = factory.Create(context.Database.GetConnectionString(),
            new DatabaseModelFactoryOptions(new string[] { }, new string[] { }));

        //VERIFY
        database.DefaultSchema.ShouldEqual(null);

        var entity = context.Model.GetEntityTypes().First();
        _output.WriteLine(config.ToString());
        _output.WriteLine($"   context.Model entity Name              = {entity.Name}");
        _output.WriteLine($"   context.Model FormSchemaTableFromModel = {entity.FormSchemaTableFromModel()}");
        _output.WriteLine($"   Scaffold.Model entity name             = {database.Tables.First().Name}");
        _output.WriteLine($"   Scaffold.Model entity schema           = {database.Tables.First().Schema}");
    }

    [Theory]
    [InlineData(MyEntityDbContext.Configs.NormalTable)]
    [InlineData(MyEntityDbContext.Configs.WholeSchemaSet)]
    public void TestSchemaDefaultNamePostgres(MyEntityDbContext.Configs config)
    {
        var options = this.CreatePostgreSqlUniqueClassOptions<MyEntityDbContext>(
            builder =>
            {
                builder.ReplaceService<IModelCacheKeyFactory, MyEntityModelCacheKeyFactory>();
                builder.UseNpgsql(this.GetUniquePostgreSqlConnectionString(), 
                    o => o.SetPostgresVersion(12, 0));
            });
        using var context = new MyEntityDbContext(options, config);
        context.Database.EnsureClean();

        var factory = context.GetDatabaseModelFactory();

        //ATTEMPT
        var database = factory.Create(context.Database.GetConnectionString(),
            new DatabaseModelFactoryOptions(new string[] { }, new string[] { }));

        //VERIFY
        database.DefaultSchema.ShouldEqual("public");
    }

    [Fact]
    public void TestReadOnlySqlServer()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<ReadOnlyDbContext>();
        using var context = new ReadOnlyDbContext(options);
        context.Database.EnsureClean();
        context.Database.ExecuteSqlRaw(
            "CREATE OR ALTER VIEW MyView AS SELECT Id, MyDateTime, MyString FROM NormalClasses");

        var factory = context.GetDatabaseModelFactory();

        //ATTEMPT
        var database = factory.Create(context.Database.GetConnectionString(),
            new DatabaseModelFactoryOptions(new string[] { }, new string[] { }));

        //VERIFY
        database.DefaultSchema.ShouldEqual("dbo");

        var entities = context.Model.GetEntityTypes();
        foreach (var entity in entities)
        {
            _output.WriteLine($"   context.Model entity Name              = {entity.Name}");
            _output.WriteLine($"   context.Model FormSchemaTableFromModel = {entity.FormSchemaTableFromModel()}");
            _output.WriteLine($"   Scaffold.Model entity name             = {database.Tables.First().Name}");
            _output.WriteLine($"   Scaffold.Model entity schema           = {database.Tables.First().Schema}");
        }
    }
}