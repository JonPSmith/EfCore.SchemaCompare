// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.BookApp.EfCode;
using Microsoft.EntityFrameworkCore;
using Test.EfHelpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class TestBookContext
    {
        [Fact]
        public void TestContextOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using var context = new BookContext(options);
            context.Database.EnsureCreated();

            //ATTEMPT 
            context.SeedDatabaseFourBooks();

            //VERIFY
            context.Books.Count().ShouldEqual(4);
            context.Model.GetDefaultSchema().ShouldEqual(null);
        }
    }
}