// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using DataLayer.OwnedTypes.EfClasses;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.OwnedTypes.EfCode
{
    public class OwnedTypeDbContext : DbContext
    {
        public enum Configs { NestedNull, NestedNotNull, SeparateTable}
        
        public Configs Config { get; }

        public OwnedTypeDbContext(DbContextOptions<OwnedTypeDbContext> options, Configs config)      
            : base(options)
        {
            Config = config;
        }

        protected override void OnModelCreating
            (ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasAlternateKey(p => p.Email);
            switch (Config)
            {
                case Configs.NestedNull:
                    modelBuilder.Entity<User>().ToTable("Users");
                    modelBuilder.Entity<User>().OwnsOne(e => e.HomeAddress);
                    break;
                case Configs.NestedNotNull:
                    modelBuilder.Entity<User>().ToTable("Users");
                    modelBuilder.Entity<User>().OwnsOne(e => e.HomeAddress);
                    modelBuilder.Entity<User>().Navigation(p => p.HomeAddress).IsRequired();
                    break;
                case Configs.SeparateTable:
                    modelBuilder.Entity<User>().ToTable("Users");
                    modelBuilder.Entity<User>().OwnsOne(e => e.HomeAddress).ToTable("Addresses");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
    }
}

