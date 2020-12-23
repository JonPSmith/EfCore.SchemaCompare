# Release notes


## TODO

- EfSchemaCompare EF Core 5 
  - Move to separate library 
  - Improve EfSchemaCompare by detecting table sharing and account for nullable properties - see [breaking change](https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-3.0/breaking-changes#dependent-entities-sharing-the-table-with-the-principal-are-now-optional).
  - EF Core 3 - Owned Type class properties are always nullable.
  - Alternative keys
  - Support Views
  - EF Core 5 - TPT
  - EF Core 5 - Owned Type class can now be required  
  - EF Core 5 - property bag
  - EF Core 5 - Same type mapped to different tables ??
  - EF Core 5 - Map entity to query
  - EF Core 5 - IPAddress and PhysicalAddress
  - Change DesignProvider to use EF Core constant instead of strings
  - Change DesignProvider: Remove Sqlite??
  - Change DesignProvider execption message to say how to use another database type
  - Check GetColumnName default - EF Core issue #23301


## 1.0.0-preview001

- Support for EF Core 5 
- Used IRelationalTypeMappingSource for constants - see EF Core #21731
- Check ValueConverters
- EF Core 5 - direct many-to-many
- EF Core 5 = persistent computed column


## Does not support

- Checking of Alternative keys
- Checking of sequences