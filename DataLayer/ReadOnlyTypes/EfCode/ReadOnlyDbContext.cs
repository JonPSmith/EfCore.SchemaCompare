// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.ReadOnlyTypes.EfClasses;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.ReadOnlyTypes.EfCode
{
    public class ReadOnlyDbContext : DbContext
    {
        public ReadOnlyDbContext(DbContextOptions<ReadOnlyDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<NormalClass> NormalClasses { get; set; }
        public DbSet<MappedToView> MappedToViews => Set<MappedToView>("MyView") ;
        public DbSet<MappedToView> MappedToTable => Set<MappedToView>("NormalClasses");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.SharedTypeEntity<MappedToView>("MyView").ToView("MyView");
            modelBuilder.SharedTypeEntity<MappedToView>("NormalClasses").ToView("NormalClasses");

            modelBuilder.Entity<MappedToQuery>().ToSqlQuery(
                @"SELECT Id, MyDateTime, MyInt, MyString FROM NormalClasses");

            modelBuilder.Entity<ReadOnlyClass>().HasNoKey();
        }
    }
}