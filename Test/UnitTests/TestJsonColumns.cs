// Copyright (c) 2024 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.JsonColumnDb;
using EfSchemaCompare;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualStudio.TestPlatform.Utilities;
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
    public void TestFindBackingField()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<JsonCustomerContext>();
        using var context = new JsonCustomerContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        //FIRST ATTEMPT
        var headEntity = context.Model.GetEntityTypes().Single(x => x.ClrType == typeof(HeadEntry));
        
        var headEntityDeclaringEntityType = headEntity.ContainingEntityType.GetNavigations()
            .Where(x => x.TargetEntityType.IsMappedToJson())
            .Select(x => x.TargetEntityType.ClrType).ToArray();
        var jsonProperties1 = headEntity.ClrType.GetProperties()
            .Where(x => headEntityDeclaringEntityType.Contains(x.PropertyType)).ToArray();

        //SECOND ATTEMPT
        var declaringEntityTypes = headEntity.ContainingEntityType.GetNavigations()
            .Where(x => x.TargetEntityType.IsMappedToJson())
            .Select(x => x.TargetEntityType.ClrType).ToArray();
        var jsonProperties2 = headEntity.ClrType.GetProperties()
            .Where(x => declaringEntityTypes.Contains(x.PropertyType)).ToArray();


        //VERIFY
        _output.WriteLine("Normal Properties");
        foreach (var property in headEntity.GetProperties())
        {
            _output.WriteLine($"  Name:{property.Name}");
        }

        _output.WriteLine("");
        _output.WriteLine("Json Properties");
        foreach (var propertyInfo in jsonProperties2)
        {
            _output.WriteLine($"  Name:{propertyInfo.Name}");
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
        IEntityType topEntityType = context.Model.GetEntityTypes().Single(x => x.ClrType == typeof(HeadEntry));
        foreach (var navigation in topEntityType.ContainingEntityType.GetNavigations()
                         .Where(x => x.TargetEntityType.IsMappedToJson()))
        {
            _output.WriteLine($"{navigation.TargetEntityType.Name} entity is stored as a Json string.");
            _output.WriteLine($"The {navigation.DeclaringEntityType.Name} has has a string to store the Json data.");
            var properties = navigation.DeclaringEntityType.GetProperties().ToArray();
            var property = properties.SingleOrDefault(x => x.ClrType.BaseType == navigation.TargetEntityType.ClrType);
            _output.WriteLine($"The property {property?.Name ?? "- None -"} has has a string to store the Json data.");
            _output.WriteLine("");
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
    public void CompareWithErrorsIgnored()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<JsonCustomerContext>();
        using var context = new JsonCustomerContext(options);
        context.Database.EnsureClean();

        var config = new CompareEfSqlConfig();
        config.IgnoreTheseErrors("EXTRA IN DATABASE: Table 'HeadEntries', column name. Found = DifferentColumnName");

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
        comparer.GetAllErrors.ShouldEqual("EXTRA IN DATABASE: Table 'HeadEntries', column name. Found = DifferentColumnName");
        _output.WriteLine(comparer.GetAllErrors);
    }
}