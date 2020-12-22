// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.BookApp.EfClasses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.BookApp.EfCode.Configurations
{
    public class BookConfig : IEntityTypeConfiguration<Book>
    {
        public void Configure
            (EntityTypeBuilder<Book> entity)
        {
            entity.Property(p => p.PublishedOn)
                .HasColumnType("date");        

            entity.Property(p => p.Price) 
                .HasColumnType("decimal(9,2)");

            entity.Property(x => x.ImageUrl) 
                .IsUnicode(false);

            entity.HasIndex(x => x.PublishedOn); 

            //Model-level query filter

            entity
                .HasQueryFilter(p => !p.SoftDeleted); 

            //----------------------------
            //relationships

            entity.HasOne(p => p.Promotion) 
                .WithOne() 
                .HasForeignKey<PriceOffer>(p => p.BookId); 

            entity.HasMany(p => p.Reviews)     
                .WithOne()                     
                .HasForeignKey(p => p.BookId); 
        }
    }

}