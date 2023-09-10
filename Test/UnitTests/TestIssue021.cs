// Copyright (c) 2023 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using EfSchemaCompare;
using Microsoft.EntityFrameworkCore;
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

    private const string AddViewSql = @"CREATE VIEW [dbo].[VW_BlogPosts]
AS
(
SELECT
    b.BlogId,
	p.PostId,
	b.[Name] AS 'BlogName',
	p.[Title] AS 'PostTitle'
FROM
    [Blogs] b
    INNER JOIN [Posts] p ON b.BlogId = p.PostId
)
";

    [Fact]
    public void CheckThatViewWorks()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<Test021DbContext>();
        using var context = new Test021DbContext(options);
        context.Database.EnsureClean();
        context.Database.ExecuteSqlRaw(AddViewSql);

        //ATTEMPT 
        context.Add(new Blog { Name = "Blog" });
        context.Add(new Post { Title = "Post" });
        context.SaveChanges();

        //VERIFY
        var view = context.VwBlogPostMany.SingleOrDefault();
        view.ShouldNotBeNull();
        view.BlogName.ShouldEqual("Blog");
        view.PostTitle.ShouldEqual("Post");
    }

    [Fact]
    public void TestCreateErrors()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<Test021DbContext>();
        using var context = new Test021DbContext(options);
        context.Database.EnsureClean();
        context.Database.ExecuteSqlRaw(AddViewSql);

        //ATTEMPT
        var comparer = new CompareEfSql();
        var hasErrors = comparer.CompareEfWithDb(context);

        //VERIFY
        hasErrors.ShouldBeTrue();
        var errors = CompareLog.ListAllErrors(comparer.Logs).ToList();
        errors.Count.ShouldEqual(2);
        errors[0].ShouldEqual("NOT IN DATABASE: VwBlogPosts->Index '', index constraint name. Expected = <null>");
        errors[1].ShouldEqual("NOT IN DATABASE: VwBlogPosts->Index '', index constraint name. Expected = <null>");
    }

    [Fact]
    public void TestErrorsSuppressed()
    {
        //SETUP
        var options = this.CreateUniqueClassOptions<Test021DbContext>();
        using var context = new Test021DbContext(options);
        context.Database.EnsureClean();
        context.Database.ExecuteSqlRaw(AddViewSql);

        //ATTEMPT
        var config = new CompareEfSqlConfig();
        config.IgnoreTheseErrors(@"NOT IN DATABASE: VwBlogPosts->Index '', index constraint name. Expected = <null>
NOT IN DATABASE: VwBlogPosts->Index '', index constraint name. Expected = <null>");
        var comparer = new CompareEfSql(config);
        var hasErrors = comparer.CompareEfWithDb(context);

        //VERIFY
        hasErrors.ShouldBeFalse(comparer.GetAllErrors);
    }

}