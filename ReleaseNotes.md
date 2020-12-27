# Release notes


## TODO

- EfSchemaCompare EF Core 5  
  - Alternative keys
  - EF Core 5 - TPT
  - EF Core 5 - Owned Type class can now be required  
  - EF Core 5 - IPAddress and PhysicalAddress
  - Change DesignProvider to use EF Core constant instead of strings
  - Change DesignProvider exception message to say how to use another database type
  - Check GetColumnName default - EF Core issue #23301


#### Next steps


## 1.0.0-preview001

- Move to separate library in order to tidy up and improve this feature
- Support for EF Core 5 database features
   - EF Core 5 - direct many-to-many
   - EF Core 5 - property bag
   - EF Core 5 - Same type mapped to different tables
   - EF Core 5 = persistent computed column
   - EF Core 5 - Entity to SQL query is marked as not checked
- Support Views
- Used IRelationalTypeMappingSource for constants - see EF Core #21731
- Fixed bug on nested Owned Type properties being nullable
- Fixed bug on nested table splitting optional class properties being nullable
- Works with ValueConverters
- Fixed bug TPH properties being nullable


## Does not support

- Cannot detect Owned Type with Required option (i.e. not null)
- Checking of Alternative keys
- Checking of sequences