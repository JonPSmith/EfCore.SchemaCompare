# EfCore.SchemaCompare

If you are changing the schema of your database's schema outside of EF Core' migrations, say by using SQL change scripts, then this library can quickly tell you if the a specific database schema and EF Core's `Model` of the database are in step.

The first number in the version number of this library defines what version of EF Core it works for. e.g. EfCore.SchemaCompare version 5 is works with to EF Core 5.

The EfCore.SchemaCompare library (shortened to EfSchemaCompare in the documentations) is available on [NuGet as EfCore.SchemaCompare](#) and is an open-source library under the MIT licence. See [ReleaseNotes](https://github.com/JonPSmith/EfCore.SchemaCompare/blob/master/ReleaseNotes.md) for details of changes and information on versions before EF Core 5.

**TABLE OF CONTENT**

1. [Introduction to how EfCore.SchemaCompare]

## List of checks

NOTE: I use the term *entity class* for classes mapped to the database by EF Core.

Here is a full list of the things that EfSchemaCompare checks

### Stage 1 - checks on EF Core side

- Table/View exist: That a table or view that an entity class is mapped exists
- Property/Column:
  - exists
  - database type, size, precision
  - nullability (see limitations)
  - computed column and its persistence
  - column default value
  - When updated, e.g. column is updated `OnAdd` for a int primary key which is provided by the database
- Primary key:
  - SQL constraint name
  - properties
- Foreign keys:
  - SQL constraint name
  - Delete behavior
  - properties
- Indexes
  - SQL constraint name
  - Unique/not unique
  - properties

### Stage 2 - checks on database

- Unused tables or views
- Unused columns
- Unused Indexes

## List of limitations

- Cannot detect [Owned Type with `Required` option](https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-5.0/whatsnew#required-11-dependents) (i.e. not null) - This is a limitation of EF Core (see EF Core issue [#23758](https://github.com/dotnet/efcore/issues/23758).
- Cannot correctly check Table-per-Type classes This is a limitation of EF Core.

The following are things I haven't bothered to check.

- Checking of Alternative keys
- Checking of sequences

## Introduction to how EfSchemaCompare works

EfSchemaCompare uses two EF Core features to get EF Core's internal schema and the database's schema. They are

- EF Core's `Model` property in your application's DbContext. This provides the internal version of the database schema that EF Core builds when 

If the database's schema and EF Core's `Model` aren't in line you can have a lot of problems.


![EfSchemaCompare diagram](https://github.com/JonPSmith/EfCore.SchemaCompare)
## How to use EfSchemaCompare

## How to suppress errors

## Other configuration options

