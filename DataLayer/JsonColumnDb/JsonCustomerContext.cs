// Copyright (c) 2024 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace DataLayer.JsonColumnDb;

public class JsonCustomerContext : DbContext
{

    public JsonCustomerContext(DbContextOptions<JsonCustomerContext> options)
        : base(options) {}

    public DbSet<HeadEntry> HeadEntries { get; set; }
    public DbSet<Normal> Normals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HeadEntry>().OwnsOne(
            headEntry => headEntry.JsonParts, ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.ToJson();
                ownedNavigationBuilder.OwnsOne(contactDetails => contactDetails.InnerJsonMap);
            });
    }
}