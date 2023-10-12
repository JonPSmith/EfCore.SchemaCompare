# Release notes

## 8.0.0-rc1-0001

- First build using NET 8-rc1

## 7.0.0

- Updated to EF Core 7.0
 
## 6.0.1

Fix to issue #21

## 6.0.1

Fix to issue #21

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