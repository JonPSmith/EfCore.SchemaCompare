// Copyright (c) 2023 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.BookApp.EfCode;
using EfSchemaCompare;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests;

public class TestIssue021
{
    //see https://github.com/JonPSmith/EfCore.SchemaCompare/issues/21#issue-1698321555
    public class Blog
    {
        public int BlogId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class VwBlogPosts
    {
        public int BlogId { get; set; }
        public int PostId { get; set; }
        public string BlogName { get; set; }
        public string PostTitle { get; set; }

        public virtual Blog Blog { get; set; }
        public virtual Post Post { get; set; }
    }

    public class Test021DbContext : DbContext
    {
        public Test021DbContext(
            DbContextOptions<Test021DbContext> options)
            : base(options) {}

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<VwBlogPosts> VwBlogPostMany { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<VwBlogPosts>(
                    eb =>
                    {
                        eb.HasNoKey();
                        eb.ToView("VW_BlogPosts");
                        eb.HasOne(t => t.Blog).WithMany().HasForeignKey(t => t.BlogId);
                        eb.HasOne(t => t.Post).WithMany().HasForeignKey(t => t.PostId);
                    });
        }
    }

    [Fact]
    public void Test()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<Test021DbContext>();
        using var context = new Test021DbContext(options);
        context.Database.EnsureClean();

        //ATTEMPT 
        var comparer = new CompareEfSql();

        //ATTEMPT
        var hasErrors = comparer.CompareEfWithDb(context);

        //VERIFY
        hasErrors.ShouldBeFalse(comparer.GetAllErrors);
    }

}