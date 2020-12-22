// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace DataLayer.OldTestSupportDbs.Issue032
{
    public class Issue32Context : DbContext
    {
        public Issue32Context(DbContextOptions<Issue32Context> options)
            : base(options) { }

        public DbSet<MyClass> MyClasses { get; set; }

        protected override void
            OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}