// Copyright (c) 2024 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.BookApp.EfCode;
using EfSchemaCompare;
using EfSchemaCompare.Internal;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests;

public class TestExtraInDatabase
{
    private readonly ITestOutputHelper _output;

    public TestExtraInDatabase(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void DecodeCompareTextToCompareLog_ExtraIndexInDatabase_Test()
    {
        //See issue #39
        var str = "EXTRA IN DATABASE: Index 'tenants', index constraint name. Found = ix_tenants_belongs_to_database_instance_id";
        var compareLog = FindErrorsToIgnore.DecodeCompareTextToCompareLog(str);
        compareLog.Type.ShouldEqual(CompareType.Index);
    }

    [Fact]
    public void TestExtraTable()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<BookContext>();
        using var context = new BookContext(options);
        context.Database.EnsureClean();

        //Add new table
        var filepath = TestData.GetFilePath("AddExtraTable.sql");
        context.ExecuteScriptFileInTransaction(filepath);

        //ATTEMPT
        var config = new CompareEfSqlConfig
        {
            TablesToIgnoreCommaDelimited = ""
        };
        var comparer = new CompareEfSql(config);
        var hasErrors = comparer.CompareEfWithDb(context);

        //VERIFY
        hasErrors.ShouldBeTrue();
        comparer.GetAllErrors.ShouldEqual(
            "EXTRA IN DATABASE: EfCore.SchemaCompare-Test_TestExtraInDatabase->Table 'ExtraTable'");
    }

    [Fact]
    public void TestExtraTableIgnore()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<BookContext>();
        using var context = new BookContext(options);
        context.Database.EnsureClean();

        //Add new table
        var filepath = TestData.GetFilePath("AddExtraTable.sql");
        context.ExecuteScriptFileInTransaction(filepath);

        //ATTEMPT
        var config = new CompareEfSqlConfig
        {
            TablesToIgnoreCommaDelimited = "",
        };
        config.IgnoreTheseErrors(
            "EXTRA IN DATABASE: EfCore.SchemaCompare-Test_TestExtraInDatabase->Table 'ExtraTable'");
        var comparer = new CompareEfSql(config);
        var hasErrors = comparer.CompareEfWithDb(context);

        //VERIFY
        hasErrors.ShouldBeFalse(comparer.GetAllErrors);
    }

    [Fact]
    public void TestExtraColumn()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<BookContext>();
        using var context = new BookContext(options);
        context.Database.EnsureClean();

        //Add new column
        var filepath = TestData.GetFilePath("AddExtraColumn.sql");
        context.ExecuteScriptFileInTransaction(filepath);

        //ATTEMPT
        var config = new CompareEfSqlConfig
        {
            TablesToIgnoreCommaDelimited = ""
        };
        var comparer = new CompareEfSql(config);
        var hasErrors = comparer.CompareEfWithDb(context);

        //VERIFY
        hasErrors.ShouldBeTrue();
        comparer.GetAllErrors.ShouldEqual(
            "EXTRA IN DATABASE: Column 'Books', column name. Found = ExtraColumn");
    }

    [Fact]
    public void TestExtraColumnIgnore()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<BookContext>();
        using var context = new BookContext(options);
        context.Database.EnsureClean();

        //Add new column
        var filepath = TestData.GetFilePath("AddExtraColumn.sql");
        context.ExecuteScriptFileInTransaction(filepath);

        //ATTEMPT
        var config = new CompareEfSqlConfig
        {
            TablesToIgnoreCommaDelimited = "",
        };
        config.IgnoreTheseErrors(
            "EXTRA IN DATABASE: Column 'Books', column name. Found = ExtraColumn");
        var comparer = new CompareEfSql(config);
        var hasErrors = comparer.CompareEfWithDb(context);

        //VERIFY
        hasErrors.ShouldBeFalse(comparer.GetAllErrors);
    }
}