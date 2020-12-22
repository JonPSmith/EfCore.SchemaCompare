// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace DataLayer.OldTestSupportDbs.Issue019
{
    public class Issue19FullCaseDbContext : DbContext
    {
        public Issue19FullCaseDbContext(DbContextOptions<Issue19FullCaseDbContext> options) : base(options) { }
        public DbSet<PrincipalClass> PrincipalClasses { get; set; }
        public DbSet<DependentClass> DependentClasses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.SetupModel(false);
        }
    }
}