// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace DataLayer.TablePerType
{
    public class TptDbContext : DbContext
    {
        public TptDbContext(DbContextOptions<TptDbContext> options)
            : base(options)
        {
        }

        public DbSet<NormalClass> NormalClasses { get; set; }

        public DbSet<TptBase> TptBases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TptVer1>().ToTable(nameof(TptVer1));
            modelBuilder.Entity<TptVer2>().ToTable(nameof(TptVer2));
        }
    }
}