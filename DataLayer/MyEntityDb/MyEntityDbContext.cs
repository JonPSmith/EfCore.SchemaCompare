// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.MyEntityDb
{
    public class MyEntityDbContext : DbContext
    {
        public enum Configs
        {
            NormalTable, TableWithSchema, WholeSchemaSet, DifferentPk, 
            ComputedCol, PersistentComputedColumn, DefaultValue,
            ShadowProp, HasIndex, HasUniqueIndex, 
            DifferentColName, StringIsRequired, StringIsAscii, Temporal
        }
        
        public Configs Config { get; }
        
        public MyEntityDbContext(DbContextOptions<MyEntityDbContext> options, Configs config)
            : base(options)
        {
            Config = config;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            switch (Config)
            {
                case Configs.NormalTable:
                    modelBuilder.Entity<MyEntity>().ToTable("MyEntites");
                    break;
                case Configs.TableWithSchema:
                    modelBuilder.Entity<MyEntity>().ToTable("MyEntities", "MySchema");
                    break;
                case Configs.WholeSchemaSet:
                    modelBuilder.Entity<MyEntity>().ToTable("MyEntites");
                    modelBuilder.HasDefaultSchema("AllSchema");
                    break;
                case Configs.DifferentPk:
                    modelBuilder.Entity<MyEntity>().ToTable("MyEntites");
                    modelBuilder.Entity<MyEntity>().HasKey(p => p.MyInt);
                    break;
                case Configs.ComputedCol:
                    modelBuilder.Entity<MyEntity>().ToTable("MyEntites");
                    modelBuilder.Entity<MyEntity>()
                        .Property(p => p.MyDateTime)
                        .HasColumnType("datetime");
                    modelBuilder.Entity<MyEntity>()
                        .Property(p => p.MyDateTime)
                        .HasComputedColumnSql("getutcdate()");
                    break;
                case Configs.PersistentComputedColumn:
                    modelBuilder.Entity<MyEntity>().ToTable("MyEntites");
                    modelBuilder.Entity<MyEntity>()
                        .Property(p => p.MyString)
                        .HasColumnType("nvarchar(30)");
                    modelBuilder.Entity<MyEntity>()
                        .Property(p => p.MyString)
                        .HasComputedColumnSql("CONVERT([nvarchar](30),[MyEntityId]+(1))", true);
                    break;
                case Configs.DefaultValue:
                    modelBuilder.Entity<MyEntity>().ToTable("MyEntites");
                    modelBuilder.Entity<MyEntity>()
                        .Property(p => p.MyString).HasDefaultValueSql("N'Hello!'");
                    modelBuilder.Entity<MyEntity>()
                        .Property(p => p.MyDateTime).HasDefaultValue(new DateTime(2000,1,1).ToString("s"));
                    break;
                case Configs.ShadowProp:
                    modelBuilder.Entity<MyEntity>().ToTable("MyEntites");
                    modelBuilder.Entity<MyEntity>().Property<int>("ShadowProp");
                    break;
                case Configs.HasIndex:
                    modelBuilder.Entity<MyEntity>().ToTable("MyEntites");
                    modelBuilder.Entity<MyEntity>().HasIndex(p => p.MyInt);
                    break;
                case Configs.HasUniqueIndex:
                    modelBuilder.Entity<MyEntity>().ToTable("MyEntites");
                    modelBuilder.Entity<MyEntity>().HasIndex(p => p.MyInt).IsUnique().HasDatabaseName("MySpecialName");
                    modelBuilder.Entity<MyEntity>().HasIndex(p => p.MyString);
                    break;
                case Configs.DifferentColName:
                    modelBuilder.Entity<MyEntity>().ToTable("MyEntites");
                    modelBuilder.Entity<MyEntity>().Property(p => p.MyInt).HasColumnName("OtherColName");
                    break;
                case Configs.StringIsRequired:
                    modelBuilder.Entity<MyEntity>().ToTable("MyEntites");
                    modelBuilder.Entity<MyEntity>().Property(p => p.MyString).IsRequired();
                    break;
                case Configs.StringIsAscii:
                    modelBuilder.Entity<MyEntity>().ToTable("MyEntites");
                    modelBuilder.Entity<MyEntity>().Property(p => p.MyString).IsUnicode(false);
                    break;
                case Configs.Temporal:
                    modelBuilder.Entity<MyEntity>().ToTable("MyEntites", b => b.IsTemporal());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}