# Release notes


## TODO

- Readme.


## 1.0.0-preview001

- Move to separate library in order to tidy up and improve this feature
- Improvement: Support for EF Core 5 database features
   - EF Core 5 - direct many-to-many
   - EF Core 5 - property bag
   - EF Core 5 - Same type mapped to different tables
   - EF Core 5 = persistent computed column
   - EF Core 5 - Entity to SQL query is marked as not checked
- New Feature: Support Views
- Improvement: Uses IRelationalTypeMappingSource for constants - see EF Core #21731
- Bug fix: Nested Owned Type properties are tested as nullable
- Bug fix: Nested table splitting optional class properties as nullable
- Bug fix: TPH properties being nullable


## Does not support

- Cannot detect Owned Type with Required option (i.e. not null)
- Cannot correctly check Table-per-Type classes
- Checking of Alternative keys
- Checking of sequences