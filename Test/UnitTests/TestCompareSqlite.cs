// Copyright (c) 2023 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.BookApp.EfCode;
using EfSchemaCompare;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests;

public class TestCompareSqlite
{
    [Fact]
    public void CompareEfSqlSqlite()
    {
        //SETUP
        var options = SqliteInMemory.CreateOptions<BookContext>();
        using var context = new BookContext(options);
        context.Database.EnsureCreated();

        var comparer = new CompareEfSql();

        //ATTEMPT
        var hasErrors = comparer.CompareEfWithDb(context);

        //VERIFY
        hasErrors.ShouldBeFalse();
    }
}