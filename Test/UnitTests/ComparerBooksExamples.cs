// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.BookApp.EfCode;
using EfSchemaCompare;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using TestSupport.Attributes;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class ComparerBooksExamples
    {
        private readonly ITestOutputHelper _output;

        public ComparerBooksExamples(ITestOutputHelper output)
        {
            _output = output;

        }

        [Fact]
        public void CompareViaContext()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<BookContext>();
            using var context = new BookContext(options);
            context.Database.EnsureClean();
            
            var comparer = new CompareEfSql();

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
        public void CompareViaConnection()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<BookContext>();
            using var context = new BookContext(options);
            context.Database.EnsureClean();
            
            var comparer = new CompareEfSql();

            //ATTEMPT
            using (new TimeThings(_output, "Time to compare simple database"))
            {
                var hasErrors = comparer.CompareEfWithDb(context.Database.GetConnectionString(), context);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }

        [Fact]
        public void CompareViaType()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<BookContext>();
            using var context = new BookContext(options);
            context.Database.EnsureClean();

            var comparer = new CompareEfSql();

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb<SqlServerDesignTimeServices>(context);

            //VERIFY
            hasErrors.ShouldBeFalse(comparer.GetAllErrors);
        }

        [Fact]
        public void CompareViaTypeWithConnection()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<BookContext>();
            using var context = new BookContext(options);
            context.Database.EnsureClean();

            var comparer = new CompareEfSql();

            //ATTEMPT
            var hasErrors =
                comparer.CompareEfWithDb<SqlServerDesignTimeServices>(context.Database.GetConnectionString(), context);

            //VERIFY
            hasErrors.ShouldBeFalse(comparer.GetAllErrors);
        }

        [Fact]
        public void CompareBadConnection()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<BookContext>();
            using var context = new BookContext(options);
            context.Database.EnsureClean();

            var comparer = new CompareEfSql();

            //ATTEMPT

            var ex = Assert.Throws<System.ArgumentException>(() =>
                comparer.CompareEfWithDb("bad connection string", context));

            //VERIFY
            ex.Message.ShouldEqual(
                "Format of the initialization string does not conform to specification starting at index 0.");
        }

        [RunnableInDebugOnly]
        public void CompareConnectionBadDatabase()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<BookContext>();
            using var context = new BookContext(options);
            context.Database.EnsureClean();
            
            var badDatabaseConnection =
                "Server=(localdb)\\mssqllocaldb;Database=BadDatabaseName;Trusted_Connection=True;MultipleActiveResultSets=true";
            var comparer = new CompareEfSql();

            //ATTEMPT
            var ex = Assert.Throws<Microsoft.Data.SqlClient.SqlException>(() =>
                comparer.CompareEfWithDb(badDatabaseConnection, context));

            //VERIFY
            ex.Message.ShouldStartWith(
                "Cannot open database \"BadDatabaseName\" requested by the login. The login failed");
        }
    }
}
