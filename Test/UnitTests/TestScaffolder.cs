// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.BookApp.EfCode;
using EfSchemaCompare;
using EfSchemaCompare.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;
using Test.Helpers;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

#pragma warning disable EF1001 // Internal EF Core API usage.
namespace Test.UnitTests
{
    public class TestScaffolder 
    {

        [Fact]
        public void GetBookContextDatabaseModel()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<BookContext>();
            using var context = new BookContext(options);
            context.Database.EnsureClean();

            var factory = context.GetDatabaseModelFactory();

            //ATTEMPT 
            var model = factory.Create(context.Database.GetConnectionString(),
                new DatabaseModelFactoryOptions(new string[] { }, new string[] { }));

            //VERIFY
            model.ShouldNotBeNull();
            model.Tables.Select(x => x.Name).OrderBy(x => x).ToArray()
                .ShouldEqual(new []{"Authors", "BookAuthor", "Books", "BookTag", "PriceOffers", "Review", "Tags" });
            model.DefaultSchema.ShouldEqual("dbo");
        }


        [Fact]
        public void GetBookContextDatabaseModelWithView()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<BookContext>();
            using var context = new BookContext(options);
            context.Database.EnsureClean();
            var filepath = TestData.GetFilePath("AddViewToBookContext.sql");
            context.ExecuteScriptFileInTransaction(filepath);

            var factory = context.GetDatabaseModelFactory();

            //ATTEMPT 
            var model = factory.Create(context.Database.GetConnectionString(),
                new DatabaseModelFactoryOptions(new string[] { }, new string[] { }));

            //VERIFY
            model.ShouldNotBeNull();
            model.Tables.Select(x => x.Name).OrderBy(x => x).ToArray()
                .ShouldEqual(new[] { "Authors", "BookAuthor", "Books", "BookTag", "MyView", "PriceOffers", "Review", "Tags" });
        }


        [Fact]
        public void TestCompareEfSqlPostgreSql()
        {
            //SETUP
            var options = this.CreatePostgreSqlUniqueClassOptions<BookContext>();
            using var context = new BookContext(options);
            context.Database.EnsureClean();

            var factory = context.GetDatabaseModelFactory();

            //ATTEMPT 
            var model = factory.Create(context.Database.GetConnectionString(),
                new DatabaseModelFactoryOptions(new string[] { }, new string[] { }));

            //VERIFY
            model.ShouldNotBeNull();
            model.Tables.Select(x => x.Name).OrderBy(x => x).ToArray()
                .ShouldEqual(new[] { "Authors", "BookAuthor", "Books", "BookTag", "PriceOffers", "Review", "Tags" });
        }
    }
}