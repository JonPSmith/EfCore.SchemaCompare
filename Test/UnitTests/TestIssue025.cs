// Copyright (c) 2023 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.MyEntityDb;
using EfSchemaCompare;
using EfSchemaCompare.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests;

public class TestIssue025
{

    public class Entity1
    {
        public int Id { get; set; }
        public string AssetId { get; set; }
    }

    public class Issue025DbContext : DbContext
    {
        public string ColumnName { get; }
        public Issue025DbContext(DbContextOptions<Issue025DbContext> options, string columnName)
            : base(options)
        {
            ColumnName = columnName;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity1>().HasIndex(e => e.AssetId)
                .IsUnique(false)
                .HasDatabaseName(ColumnName);
        }
    }

    //see https://docs.microsoft.com/en-us/ef/core/modeling/dynamic-model
    public class Issue025ModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime)
            => context is Issue025DbContext dynamicContext
                ? (context.GetType(), dynamicContext.ColumnName, designTime)
                : (object)context.GetType();

        public object Create(DbContext context)
            => Create(context, false);
    }

    [Fact]
    public void ColumnOk()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<Issue025DbContext>(
            builder => builder.ReplaceService<IModelCacheKeyFactory, Issue025ModelCacheKeyFactory>());
        using (var context = new Issue025DbContext(options, "First"))
        {
            context.Database.EnsureCreated();
        }

        using (var context = new Issue025DbContext(options, "First"))
        {
            //ATTEMPT
            var comparer = new CompareEfSql();
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeFalse();
        }
    }

    [Fact]
    public void ColumnNotInDatabase()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<Issue025DbContext>(
            builder => builder.ReplaceService<IModelCacheKeyFactory, Issue025ModelCacheKeyFactory>());
        using (var context = new Issue025DbContext(options, "First"))
        {
            context.Database.EnsureCreated();
        }

        using (var context = new Issue025DbContext(options, "Second"))
        {
            //ATTEMPT
            var comparer = new CompareEfSql();
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeTrue();
            var errors = CompareLog.ListAllErrors(comparer.Logs).ToList();
            errors.Single().ShouldEqual("NOT IN DATABASE: Entity1->Index 'AssetId', index constraint name. Expected = Second");
        }
    }
}