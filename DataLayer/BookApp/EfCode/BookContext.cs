// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.BookApp.EfClasses;
using DataLayer.BookApp.EfCode.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.BookApp.EfCode
{
    public class BookContext : DbContext
    {
        public enum Configs { M2MDict, M2MProvided}

        public BookContext(                             
            DbContextOptions<BookContext> options, Configs config = Configs.M2MDict)      
            : base(options)
        {
            Config = config;
        }

        public Configs Config { get; }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<PriceOffer> PriceOffers { get; set; }

        public DbSet<Tag> Tags { get; set; }

        protected override void
            OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BookConfig());       
            modelBuilder.ApplyConfiguration(new BookAuthorConfig()); 
            modelBuilder.ApplyConfiguration(new PriceOfferConfig());

            if (Config == Configs.M2MProvided)
            {
                modelBuilder.Entity<Book>().HasMany(x => x.Tags)
                    .WithMany(x => x.Books)
                    .UsingEntity<BookTag>(
                        x => x.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId),
                        x => x.HasOne(x => x.Book).WithMany().HasForeignKey(x => x.BookId));
            }
        }
    }
}

