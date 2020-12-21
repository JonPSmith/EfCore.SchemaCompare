# Release notes


## TODO

- EfSchemaCompare EF Core 5 
  - Move to separate library 
  - Improve EfSchemaCompare by detecting table sharing and account for nullable properties - see [breaking change](https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-3.0/breaking-changes#dependent-entities-sharing-the-table-with-the-principal-are-now-optional).
  - EF Core 3 - Owned Type class properties are always nullable.
  - ValueConverters
  - Support Views
  - Use IRelationalTypeMappingSource for constants - see EF Core #21731
  - EF Core 5 - TPT
  - EF Core 5 - Owned Type class can now be required
  - EF Core 5 - direct many-to-many
  - EF Core 5 - property bag
  - EF Core 5 - Same type mapped to different tables ??
  - EF Core 5 = persistant computed column
  - EF Core 5 - Map entity to query
  - EF Core 5 - IPAddress and PhysicalAddress
  - Change DesignProvider to use EF Core constant instead of strings
  - Change DesignProvider: Remove Sqlite??
  - Change DesignProvider execption message to say how to use another database type


## 1.0.0-preview001

