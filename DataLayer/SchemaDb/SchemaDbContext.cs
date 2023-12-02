// Copyright (c) 2023 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace DataLayer.SchemaDb;

public class SchemaDbContext : DbContext
{
    public SchemaDbContext(DbContextOptions<SchemaDbContext> options)
        : base(options) { }

    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void
        OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>().ToTable("SchemaTest");
        modelBuilder.Entity<Author>().ToTable("SchemaTest", "Schema1");
        modelBuilder.Entity<Review>().ToTable("SchemaTest", "Schema2");
    }
}