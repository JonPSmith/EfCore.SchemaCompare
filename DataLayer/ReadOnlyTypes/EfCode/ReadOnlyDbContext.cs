// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.ReadOnlyTypes.EfClasses;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.ReadOnlyTypes.EfCode
{
    public class ReadOnlyDbContext : DbContext
    {
        public enum Configs { MappedToViewClass, BadMappedToViewClass }

        public Configs Config { get; }
        
        public ReadOnlyDbContext(DbContextOptions<ReadOnlyDbContext> options, Configs config = Configs.MappedToViewClass)
            : base(options)
        {
            Config = config;
        }
        
        public DbSet<NormalClass> NormalClasses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.SharedTypeEntity<MappedToView>("NormalClasses").ToView("NormalClasses");
            
            if (Config == Configs.BadMappedToViewClass)
                modelBuilder.Entity<MappedToViewBad>().ToView("MyView");
            else
                modelBuilder.SharedTypeEntity<MappedToView>("MyView").ToView("MyView");

            modelBuilder.Entity<MappedToQuery>().ToSqlQuery(
                @"SELECT Id, MyDateTime, MyInt, MyString FROM NormalClasses");

            modelBuilder.Entity<ReadOnlyClass>().HasNoKey();
        }
    }
}