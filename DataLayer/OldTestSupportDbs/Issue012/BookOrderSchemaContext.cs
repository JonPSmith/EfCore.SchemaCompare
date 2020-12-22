// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.BookApp.EfClasses;
using DataLayer.BookApp.EfCode.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.OldTestSupportDbs.Issue012
{
    public class Issue012DbContext : DbContext
    {
        public Issue012DbContext(                             
            DbContextOptions<Issue012DbContext> options)      
            : base(options) {}

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<PriceOffer> PriceOffers { get; set; }

        protected override void
            OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BookConfig());       
            modelBuilder.ApplyConfiguration(new BookAuthorConfig()); 
            modelBuilder.ApplyConfiguration(new PriceOfferConfig());

            modelBuilder.Entity<Book>().ToTable("DupTable", "BookSchema");
            modelBuilder.Entity<Author>().ToTable("DupTable", "OrderSchema");
        }
    }
}

