// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.Chapter09ViewUsage;
using EfSchemaCompare;
using Microsoft.EntityFrameworkCore;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class ComparerChapter09ViewUsage
    {
        private readonly ITestOutputHelper _output;

        public ComparerChapter09ViewUsage(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void EmulateAFiveStepMigrationOfContinuousRunningAppOk()
        {
            //SETUP
            var app1Options = this.CreateUniqueClassOptions<App1DbContext>();
            var app2Options = this.CreateUniqueClassOptions<App2DbContext>();
            //APP1 RUNNING
            using (var app1Context = new App1DbContext(app1Options))
            {
                app1Context.Database.EnsureClean();

                //APP1
                app1Context.Add(new UserPart1
                    {Name = "Added by App1, step1", Street = "App1 street", City = "App1 city"});
                app1Context.SaveChanges();

                //APPLY 1st migration
                Migration1(app1Context);

                //APP2 RUNNING while APP2 is still running
                using (var app2Context = new App2DbContext(app2Options))
                {
                    var comparer = new CompareEfSql();

                    //ATTEMPT
                    var hasErrors = comparer.CompareEfWithDb(app2Context);

                    //VERIFY
                    hasErrors.ShouldBeFalse(comparer.GetAllErrors);
                }
            }
        }

        private void Migration1(DbContext context)
        {
            context.Database.ExecuteSqlRaw(@"CREATE TABLE [Addresses] (
    [AddressId] int NOT NULL IDENTITY,
    [Street] nvarchar(max) NULL,
    [City] nvarchar(max) NULL,
    CONSTRAINT [PK_Addresses] PRIMARY KEY ([AddressId])
);");
            context.Database.ExecuteSqlRaw("ALTER TABLE [Users] ADD [AddressId] int NULL");
            context.Database.ExecuteSqlRaw("ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_Addresses_AddressId] " +
                                           "FOREIGN KEY ([AddressId]) REFERENCES [Addresses] ([AddressId]) ON DELETE NO ACTION");
            context.Database.ExecuteSqlRaw("CREATE INDEX [IX_Users_AddressId] ON [Users] ([AddressId]);");

            context.Database.ExecuteSqlRaw(@"CREATE VIEW [GetUserWithAddress] AS
SELECT UserId,
   Name,
   CASE
      WHEN users.AddressId IS NULL THEN Street
	  ELSE (Select Street FROM Addresses AS addr WHERE users.AddressId = addr.AddressId)
	END AS Street,
	CASE
      WHEN AddressId IS NULL THEN City
	  ELSE (Select City FROM Addresses AS addr WHERE users.AddressId = addr.AddressId)
	END AS City
FROM Users as users");
        }
    }
    
    
}
