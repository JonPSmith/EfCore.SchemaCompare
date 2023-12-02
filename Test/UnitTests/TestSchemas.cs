// Copyright (c) 2023 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using DataLayer.MyEntityDb;
using DataLayer.SchemaDb;
using EfSchemaCompare;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests;

public class TestSchemas
{
    private readonly ITestOutputHelper _output;

    public TestSchemas(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void CompareEfSqlServer()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<SchemaDbContext>();
        using var context = new SchemaDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var comparer = new CompareEfSql();

        //ATTEMPT
        var hasErrors = comparer.CompareEfWithDb(context);

        //VERIFY
        hasErrors.ShouldBeFalse();
    }

    [Fact]
    public void CompareEfSqlServerExcludeTableWithDefaultSchema()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<SchemaDbContext>();
        using var context = new SchemaDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var config = new CompareEfSqlConfig
        {
            TablesToIgnoreCommaDelimited = "SchemaTest"
        };
        var comparer = new CompareEfSql(config);

        //ATTEMPT
        var hasErrors = comparer.CompareEfWithDb(context);

        //VERIFY
        _output.WriteLine(comparer.GetAllErrors);
        hasErrors.ShouldBeTrue();
        comparer.GetAllErrors.ShouldEqual("NOT IN DATABASE: Entity 'Book', table name. Expected = SchemaTest");
    }

    [Fact]
    public void CompareEfSqlServerExcludeTableWithSchema()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<SchemaDbContext>();
        using var context = new SchemaDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var config = new CompareEfSqlConfig
        {
            TablesToIgnoreCommaDelimited = "Schema2.SchemaTest"
        };
        var comparer = new CompareEfSql(config);

        //ATTEMPT
        var hasErrors = comparer.CompareEfWithDb(context);

        //VERIFY
        _output.WriteLine(comparer.GetAllErrors);
        hasErrors.ShouldBeTrue();
        comparer.GetAllErrors.ShouldEqual("NOT IN DATABASE: Entity 'Review', table name. Expected = Schema2.SchemaTest");
    }

    [Fact]
    public void CompareEfSqlServerExcludeMultipleTables()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<SchemaDbContext>();
        using var context = new SchemaDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var config = new CompareEfSqlConfig
        {
            TablesToIgnoreCommaDelimited = "SchemaTest,Schema1.SchemaTest , Schema2.SchemaTest "
        };
        var comparer = new CompareEfSql(config);

        //ATTEMPT
        var hasErrors = comparer.CompareEfWithDb(context);

        //VERIFY
        _output.WriteLine(comparer.GetAllErrors);
        hasErrors.ShouldBeTrue();
        comparer.GetAllErrors.ShouldEqual(@"NOT IN DATABASE: Entity 'Author', table name. Expected = Schema1.SchemaTest
NOT IN DATABASE: Entity 'Book', table name. Expected = SchemaTest
NOT IN DATABASE: Entity 'Review', table name. Expected = Schema2.SchemaTest");
    }

    [Fact]
    public void CompareEfSqlServerExcludeBadTable()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<SchemaDbContext>();
        using var context = new SchemaDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var config = new CompareEfSqlConfig
        {
            TablesToIgnoreCommaDelimited = "BadTableName"
        };
        var comparer = new CompareEfSql(config);

        //ATTEMPT
        try
        {
            comparer.CompareEfWithDb(context);
        }
        catch (Exception e)
        {
            e.Message.ShouldEqual("The TablesToIgnoreCommaDelimited config property contains a table name of 'BadTableName', which was not found in the database");
            return;
        }

        //VERIFY
        true.ShouldBeFalse();
    }

    [Fact]
    public void CompareEfPostgreSqlExcludeMultipleTables()
    {
        //SETUP
        var options = this.CreatePostgreSqlUniqueClassOptions<SchemaDbContext>(
            builder =>
            {
                builder.UseNpgsql(this.GetUniquePostgreSqlConnectionString(),
                    o => o.SetPostgresVersion(12, 0));
            });
        using var context = new SchemaDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var config = new CompareEfSqlConfig
        {
            TablesToIgnoreCommaDelimited = "SchemaTest,Schema1.SchemaTest , Schema2.SchemaTest "
        };
        var comparer = new CompareEfSql(config);

        //ATTEMPT
        var hasErrors = comparer.CompareEfWithDb(context);

        //VERIFY
        _output.WriteLine(comparer.GetAllErrors);
        hasErrors.ShouldBeTrue();
        comparer.GetAllErrors.ShouldEqual(@"NOT IN DATABASE: Entity 'Author', table name. Expected = Schema1.SchemaTest
NOT IN DATABASE: Entity 'Book', table name. Expected = SchemaTest
NOT IN DATABASE: Entity 'Review', table name. Expected = Schema2.SchemaTest");
    }
}