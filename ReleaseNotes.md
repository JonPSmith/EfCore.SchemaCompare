# Release notes

## 8.2.0

- The ExtraInDatabase errors didn't correctly say what part of the database (e.g. "Table", "Column") was extra. This is fixed. 
- NOTE: If you have ignored some ExtraInDatabase errors you our old ignored ExtraInDatabase might need updating to the new (correct format) pattern.  
- New boolean 'AlwaysRunStage2' in the CompareEfSqlConfig. If set to 'true', then Stage 2 will always run, even if there are non-ignored errors. See issue #38, which made me add this new feature.


## 8.1.0

- New feature: Checks Json Mapping data, see README for more. Limitation that only works with the default column name
- Fixed issue #18. i.e "columns that are extra in db are not excluded" is fixed
- IEntityType is obsolete, moved to ITypeBase - see https://github.com/dotnet/efcore/issues/34594
- Updated vunables NuGets System.Text.Json and Microsoft.Extensions.Caching.Memory

## 8.0.4

- REALLY removed Microsoft.IdentityModel.Tokens NuGet as not correct (see issue #36)

## 8.0.3: ERROR - use 8.0.4

- Removed Microsoft.IdentityModel.Tokens NuGet as not correct (see issue #36)

## 8.0.2

- Removed vulnerable NuGets
- Removed EF Core database NuGets, e.g. Microsoft.EntityFrameworkCore.SqlServer, because its uses provided DbContext

## 8.0.1

- Fixed problems with TablesToIgnoreCommaDelimited with databases not supporting schema - see issue #30

## 8.0.0

- Supports .NET 8
- NEW FEATURE: Now compares all EF Core database providers, but some database providers may show incorrect match errors. Thanks to users GitHub bgrauer-atacom and @lweberprb for this feature.
- BREAKING CHANGE: You need to add the Microsoft.EntityFrameworkCore.Design NuGet to the application that uses this library 

## 8.0.0-rc2-0002

- Supports .NET 8-rc.2 - please try this version and report any problems
- NEW FEATURE: Now compares all EF Core database providers, but some database providers may show incorrect match errors. Thanks to users GitHub bgrauer-atacom and @lweberprb for this feature.
- BREAKING CHANGE: You need to add the Microsoft.EntityFrameworkCore.Design NuGet to your 
- Bug Fix: Fix to issue #21

## 7.0.0

- Updated to EF Core 7.0
 
## 6.0.2

- Fix to issue #21, but now catches "Not In Database" errors, as seen in issue #25

## 6.0.1

- Fix to issue #21

## 6.0.0

- Updated to EF Core 6.0 (thanks to Wade Baglin, GitHub @pleb)
- Added TemporalTable to compare (thanks to Wade Baglin, GitHub @pleb)

## 6.0.0-preview001

- Updated to .NET 6.0 preview RC.2 (thanks to Wade Baglin, GitHub @pleb)
- Added TemporalTable to compare (thanks to Wade Baglin, GitHub @pleb)

## 5.1.4

- Removed development dependency of package. It made it harder to EfCore.SchemaCompare in unit tests

## 5.1.3

- Make package a development dependency - see issue #4
- Updated the exception message about how to use another database type.

## 5.1.2

- Bug fix - looking for appsettings.json file throws an exception if it can't find the folder. See issue #3

## 5.1.1

- Bug fix - Table with incorrect name threw an exception. See issue #2

## 5.1.0

- Fixed bug in Owned Types with `IsRequired` where properties aren't forced to nullable

## 5.0.1

- Fixed bug in table checks

## 5.0.0

- Moved from EfCore.TestSupport to become a separate library 
- Big tidy up and improve this feature
- Improvement: Support for EF Core 5 database features
   - EF Core 5 - direct many-to-many
   - EF Core 5 - property bag
   - EF Core 5 - Same type mapped to different tables
   - EF Core 5 - persistent computed column
   - EF Core 5 - Entity to SQL query is marked as not checked
- New Feature: Checks Views
- Improvement: Uses IRelationalTypeMappingSource for constants - see EF Core #21731
- Bug fix: Nested Owned Type properties are tested as nullable
- Bug fix: Nested table splitting optional class properties as nullable
- Bug fix: TPH properties being nullable
- BREAKING CHANGE: Cannot compare tables/views using IgnoreCase

### Does not support

- Cannot detect Owned Type with Required option (i.e. not null)
- Cannot correctly check Table-per-Type classes
- You can no longer compare database entries using ignore case. (that is a EF Core 5 limitation)
- Checking of collations
- Checking of Alternative keys
- Checking of sequences


## Previous releases of EfSchemaCompare

This library started with EF Core 5. For EF Core below 5.0.0, for instance EF Core 3.x, of EF Core 2.1.x then use [EfCore.TestSupport](https://github.com/JonPSmith/EfCore.TestSupport)