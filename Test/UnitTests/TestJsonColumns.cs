// Copyright (c) 2024 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.JsonColumnDb;
using EfSchemaCompare;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests;

public class TestJsonColumns
{
    private readonly ITestOutputHelper _output;

    public TestJsonColumns(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void CheckJsonCustomerContextWorks()
    {
        //SETUP
        var options = this.CreateUniqueClassOptionsWithLogTo<JsonCustomerContext>(_output.WriteLine);
        using var context = new JsonCustomerContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        //ATTEMPT
        context.Add(new HeadEntry
        {
            HeadInt = -1,
            TopJsonMap = new TopJsonMap { TopString = "Top String",
                MiddleJsonMap = new MiddleJsonMap
                {
                    MiddleJsonString = "Middle String",
                    BottomJsonMap = new BottomJsonMap{ BottomJsonString = "Bottom string"}
                }},
            ExtraJsonParts = new ExtraJson{ ExtraString = "Extra String", ExtraInt = 123}
        });
        context.Add(new Normal { NormalString = "Normal", NormalExtra = new NormalExtra { ExtraString = "Extra"}});
        context.SaveChanges();

        //VERIFY
        context.ChangeTracker.Clear();
        context.HeadEntries.Single().ToString().ShouldEqual(
            "HeadInt: -1, TopJsonMap: Top String, Middle String, Bottom string, ExtraJsonParts: ExtraString: Extra String, ExtraInt: 123");
        var normal = context.Normals.Include(x => x.NormalExtra).Single();
        normal.NormalString.ShouldEqual("Normal");
        normal.NormalExtra.ExtraString.ShouldEqual("Extra");
    }

    [Fact]
    public void ShowEntitiesWithIsJson()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<JsonCustomerContext>();
        using var context = new JsonCustomerContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        //ATTEMPT
        foreach (var entityType in context.Model.GetEntityTypes())
        {
            _output.WriteLine($"Entity Name: {entityType.Name}");
            var properties = entityType.GetProperties();
            foreach (var property in properties)
            {
                _output.WriteLine($"  Property = {property.Name}, type = {property.GetColumnType()}");
            }

            var navigations = entityType.ContainingEntityType.GetNavigations().ToArray();
            if (navigations.Any())
            {
                var num = 1;
                foreach (var navigation in navigations)
                {
                    _output.WriteLine($"  {num++}");
                    _output.WriteLine($"  Navigation = {navigation.Name}, " +
                                      $"IsJson = {navigation.TargetEntityType.IsMappedToJson()}");
                    _output.WriteLine($"     TargetEntityType = {navigation.TargetEntityType.Name}, ");
                    _output.WriteLine($"     DeclaringEntityType = {navigation.DeclaringEntityType.Name}");
                }
            }
        }
    }

    [Fact]
    public void DetectJsonMappedEntities()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<JsonCustomerContext>();
        using var context = new JsonCustomerContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        //ATTEMPT
        foreach (var entityType in context.Model.GetEntityTypes())
        {
            foreach (var navigation in entityType.ContainingEntityType.GetNavigations()
                         .Where(x => x.TargetEntityType.IsMappedToJson()))
            {
                _output.WriteLine($"{navigation.TargetEntityType.Name} entity is stored as a Json string.");
                _output.WriteLine($"And the {navigation.DeclaringEntityType.Name} has the string holding the Json.");
                _output.WriteLine("");
            }
        }
    }

    [Fact]
    public void CheckNavigations()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<JsonCustomerContext>();
        using var context = new JsonCustomerContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        //ATTEMPT
        foreach (var entityType in context.Model.GetEntityTypes())
        {
            _output.WriteLine($"Entity Name: {entityType.Name}");
            var properties = entityType.GetProperties();
            foreach (var property in properties)
            {
                _output.WriteLine($"  Property = {property.Name}, type = {property.GetColumnType()}");
            }
            var containerNavs = entityType.ContainingEntityType.GetNavigations().ToArray();
            if (containerNavs.Any())
            {
                var num = 1;
                foreach (var navigation in containerNavs)
                {
                    _output.WriteLine($"  {num++}");
                    _output.WriteLine($"  Navigation = {navigation.Name}, " +
                                      $"IsJson = {navigation.TargetEntityType.IsMappedToJson()}");
                    _output.WriteLine($"     TargetEntityType = {navigation.TargetEntityType.Name}, ");
                    _output.WriteLine($"     DeclaringEntityType = {navigation.DeclaringEntityType.Name}");
                }
            }
        }

        //VERIFY
    }

    [Fact]
    public void CompareWithStage1ErrorsIgnored()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<JsonCustomerContext>();
        using var context = new JsonCustomerContext(options);
        context.Database.EnsureClean();

        var config = new CompareEfSqlConfig();
        config.IgnoreTheseErrors(
            @"DIFFERENT: TopJsonMap->PrimaryKey '- no primary key -', constraint name. Expected = - no primary key -, found = PK_HeadEntries
DIFFERENT: Entity 'TopJsonMap', constraint name. Expected = - no primary key -, found = PK_HeadEntries
DIFFERENT: ContainsJsonMaps->PrimaryKey '- no primary key -', constraint name. Expected = - no primary key -, found = PK_HeadEntries
NOT IN DATABASE: ContainsJsonMaps->ForeignKey 'FK_HeadEntries_HeadEntries_HeadEntryId', constraint name. Expected = FK_HeadEntries_HeadEntries_HeadEntryId
DIFFERENT: Entity 'ContainsJsonMaps', constraint name. Expected = - no primary key -, found = PK_HeadEntries");
//EXTRA IN DATABASE: Table 'HeadEntries', column name. Found = TopJsonMap
//EXTRA IN DATABASE: Table 'HeadEntries', column name. Found = TopJsonMap
//EXTRA IN DATABASE: Table 'HeadEntries', column name. Found = TopJsonMap

        var comparer = new CompareEfSql(config);

        //ATTEMPT
        //This will compare EF Core model of the database 
        //with the database that the context's connection points to
        var hasErrors = comparer.CompareEfWithDb(context);

        //VERIFY
        //Only stage 1 errors are removed 
        hasErrors.ShouldBeTrue();
        comparer.GetAllErrors.ShouldEqual(@"EXTRA IN DATABASE: Table 'HeadEntries', column name. Found = TopJsonMap
EXTRA IN DATABASE: Table 'HeadEntries', column name. Found = TopJsonMap
EXTRA IN DATABASE: Table 'HeadEntries', column name. Found = TopJsonMap");
    }

    [Fact]
    public void CompareWithStage1And2ErrorsIgnored()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<JsonCustomerContext>();
        using var context = new JsonCustomerContext(options);
        context.Database.EnsureClean();

        var config = new CompareEfSqlConfig();
        config.IgnoreTheseErrors(
            @"DIFFERENT: TopJsonMap->PrimaryKey '- no primary key -', constraint name. Expected = - no primary key -, found = PK_HeadEntries
DIFFERENT: Entity 'TopJsonMap', constraint name. Expected = - no primary key -, found = PK_HeadEntries
DIFFERENT: ContainsJsonMaps->PrimaryKey '- no primary key -', constraint name. Expected = - no primary key -, found = PK_HeadEntries
NOT IN DATABASE: ContainsJsonMaps->ForeignKey 'FK_HeadEntries_HeadEntries_HeadEntryId', constraint name. Expected = FK_HeadEntries_HeadEntries_HeadEntryId
DIFFERENT: Entity 'ContainsJsonMaps', constraint name. Expected = - no primary key -, found = PK_HeadEntries
EXTRA IN DATABASE: Table 'HeadEntries', column name. Found = TopJsonMap
EXTRA IN DATABASE: Table 'HeadEntries', column name. Found = TopJsonMap
EXTRA IN DATABASE: Table 'HeadEntries', column name. Found = TopJsonMap");

        var comparer = new CompareEfSql(config);

        //ATTEMPT
        //This will compare EF Core model of the database 
        //with the database that the context's connection points to
        var hasErrors = comparer.CompareEfWithDb(context);

        //VERIFY
        //The CompareEfWithDb method returns true if there were errors. 
        //The comparer.GetAllErrors property returns a string
        //where each error is on a separate line
        hasErrors.ShouldBeFalse(comparer.GetAllErrors);
    }

    [Fact]
    public void CompareWithNoIgnores()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<JsonCustomerContext>();
        using var context = new JsonCustomerContext(options);
        context.Database.EnsureClean();

        var comparer = new CompareEfSql();

        //ATTEMPT
        var hasErrors = comparer.CompareEfWithDb(context);

        //VERIFY
        //Just print errors
        hasErrors.ShouldBeTrue();
        _output.WriteLine(comparer.GetAllErrors);
    }
}