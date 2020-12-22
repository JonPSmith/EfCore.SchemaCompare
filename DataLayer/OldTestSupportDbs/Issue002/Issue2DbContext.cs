// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace DataLayer.OldTestSupportDbs.Issue002
{
    public class Issue2DbContext : DbContext
    {
        public Issue2DbContext(
            DbContextOptions<Issue2DbContext> options)      
            : base(options) { }

        public DbSet<NormativeReference> NormativeReferences { get; set; }
        public DbSet<PrimaryKeyGuid> PrimaryKeyGuids { get; set; }
        public DbSet<PrincipalEntity> PrincipalEntities { get; set; }
    }
}