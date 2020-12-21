// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class Issue23301Test
    {
        public class MyClass
        {
            public int Id { get; set; }
        }

        public class MyContext : DbContext
        {
            public MyContext(
                DbContextOptions<MyContext> options)
                : base(options) { }

            public DbSet<MyClass> MyClasses { get; set; }
        }

        [Fact]
        public void TestModelGetColumnName()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<MyContext>();
            using var context = new MyContext(options);

            var modelEntity = context.Model.GetEntityTypes().Single();
            var modelProperty = modelEntity.GetProperties().Single();

            //ATTEMPT
            var original = modelProperty.GetColumnName();
            var storeObjectSchemaNull = modelProperty.GetColumnName(StoreObjectIdentifier.Table("MyClasses", null));
            var storeObjectSchemaDbo = modelProperty.GetColumnName(StoreObjectIdentifier.Table("MyClasses", "dbo"));

            //VERIFY
            context.Model.GetDefaultSchema().ShouldEqual(null);
            original.ShouldEqual(nameof(MyClass.Id));
            storeObjectSchemaNull.ShouldEqual(nameof(MyClass.Id));
            storeObjectSchemaDbo.ShouldEqual(null);
        }
    }
}