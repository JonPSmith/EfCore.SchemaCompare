// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace DataLayer.OldTestSupportDbs.Issue003
{
    public class Issue3DbContext : DbContext
    {
        public Issue3DbContext(
            DbContextOptions<Issue3DbContext> options)      
            : base(options) { }

        public DbSet<Parameter> Parameters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parameter>()
                .Property(p => p.ValueAggregationTypeId).HasDefaultValue(ValueAggregationTypeEnum.Invariable);
        }
    }
}