// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.BookApp.EfCode;
using EfSchemaCompare.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class TestBookContext
    {

        [Fact]
        public void Test()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using var context = new BookContext(options);
            context.Database.EnsureCreated();

            //ATTEMPT 
            var dbProvider = context.GetService<IDatabaseProvider>();
            var x = context.GetService<IDiagnosticsLogger < DbLoggerCategory.Scaffolding >>();
            var databaseProvider = new SqlServerDatabaseModelFactory(x);

            //VERIFY
        }


        [Fact]
        public void TestSeedContextOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using var context = new BookContext(options);
            context.Database.EnsureCreated();

            //ATTEMPT 
            context.SeedDatabaseFourBooks();

            //VERIFY
            context.Books.Count().ShouldEqual(4);
            context.Tags.Count().ShouldEqual(4);
            context.Model.GetDefaultSchema().ShouldEqual(null);
        }

        [Fact]
        public void TestTagsOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using var context = new BookContext(options);
            context.Database.EnsureCreated();
            context.SeedDatabaseFourBooks();

            context.ChangeTracker.Clear();
            
            //ATTEMPT 
            var books = context.Books.Include(b => b.Tags).ToList();

            //VERIFY
            var bTags = books.Select(x => new {
                x.BookId, 
                tagsNames = string.Join(" | ", x.Tags.Select(y => y.TagId))

            });
        }

        [Fact]
        public void TestContextModelOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using var context = new BookContext(options);
            context.Database.EnsureCreated();

            //ATTEMPT 
            var tables = context.Model.GetEntityTypes().Select(x => x.GetTableName()).ToArray();

            //VERIFY
            tables.ShouldEqual(new []{ "BookTag", "Authors", "Books", "BookAuthor", "PriceOffers", "Review", "Tags" });
        }

        [Fact]
        public void TestContextModelGetRelationalModelOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using var context = new BookContext(options);
            context.Database.EnsureCreated();

            //ATTEMPT 
            var relational = context.Model.GetRelationalModel();

            //VERIFY
        }

        [Fact]
        public void TestDatabaseTablesOk()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<BookContext>();
            using var context = new BookContext(options);
            context.Database.EnsureClean();
            context.Database.EnsureCreated();

            //ATTEMPT 
            var factory = context.GetDatabaseModelFactory();

            //ATTEMPT 

            var model = factory.Create(context.Database.GetConnectionString(),
                new DatabaseModelFactoryOptions(new string[] { }, new string[] { }));

            //VERIFY
            model.ShouldNotBeNull();
            model.Tables.Select(x => x.Name).ToArray()
                .ShouldEqual(new[] { "Authors", "BookAuthor", "Books", "BookTag", "PriceOffers", "Review", "Tags" });
        }
    }
}