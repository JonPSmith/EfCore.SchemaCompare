// Copyright (c) 2023 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.BookApp.EfCode;
using EfSchemaCompare;
using Microsoft.EntityFrameworkCore;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests;

public class TestCompareSqlite
{
    private readonly ITestOutputHelper _output;

    public TestCompareSqlite(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void CompareEfSqlSqlite_FAILS() //SQLite FK names are null: see dotnet/efcore#8802
    {
        //SETUP
        var builder = new DbContextOptionsBuilder<BookContext>()
            .UseSqlite($"Data Source={TestData.GetTestDataDir()}/CompareEfSqlSqlite.db");
        using var context = new BookContext(builder.Options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var comparer = new CompareEfSql();

        //ATTEMPT
        var hasErrors = comparer.CompareEfWithDb(context);

        //VERIFY
        _output.WriteLine(comparer.GetAllErrors);
        hasErrors.ShouldBeFalse();
    }

    [Fact]
    public void CompareEfSqlSqliteIgnore_FAILS() //SQLite FK names are null: see dotnet/efcore#8802
    {
        //SETUP
        var builder = new DbContextOptionsBuilder<BookContext>()
            .UseSqlite($"Data Source={TestData.GetTestDataDir()}/CompareEfSqlSqliteIgnore.db");
        using var context = new BookContext(builder.Options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        //NOTE: the ignore of a tables DOES work with no schema
        var config = new CompareEfSqlConfig
        {
            TablesToIgnoreCommaDelimited = "Review"
        };
        var comparer = new CompareEfSql(config);

        //ATTEMPT
        var hasErrors = comparer.CompareEfWithDb(context);

        //VERIFY
        _output.WriteLine(comparer.GetAllErrors);
        hasErrors.ShouldBeTrue();
    }
}