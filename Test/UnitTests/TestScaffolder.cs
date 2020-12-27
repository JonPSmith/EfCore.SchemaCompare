// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using DataLayer.BookApp.EfCode;
using EfSchemaCompare;
using EfSchemaCompare.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

#pragma warning disable EF1001 // Internal EF Core API usage.
namespace Test.UnitTests
{
    public class TestScaffolder 
    {

        [Fact]
        public void GetDatabaseModel()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<BookContext>();
            using var context = new BookContext(options);
            context.Database.EnsureCreated();

            var serviceProvider = new SqlServerDesignTimeServices().GetDesignTimeProvider();
            var factory = serviceProvider.GetService<IDatabaseModelFactory>();

            //ATTEMPT 
            var model = factory.Create(context.Database.GetConnectionString(),
                new DatabaseModelFactoryOptions(new string[] { }, new string[] { }));

            //VERIFY
            model.ShouldNotBeNull();
            model.DefaultSchema.ShouldEqual("dbo");
        }


        [Fact]
        public void TestCompareEfSqlPostgreSql()
        {
            //SETUP
            var builder = new DbContextOptionsBuilder<BookContext>()
                .UseNpgsql(null);
            using var context = new BookContext(builder.Options);

            var comparer = new CompareEfSql();

            //ATTEMPT 
            var ex = Assert.Throws<InvalidOperationException>(() => comparer.CompareEfWithDb<NpgsqlDesignTimeServices>(context));

            //VERIFY
            ex.Message.ShouldEqual("A relational store has been configured without specifying either the DbConnection or connection string to use.");
        }
    }
}