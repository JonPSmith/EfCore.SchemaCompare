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

- Cannot detect [Owned Type with `Required` option](https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-5.0/whatsnew#required-11-dependents) (i.e. not null) - This is a limitation of EF Core (see EF Core issue [#23758](https://github.com/dotnet/efcore/issues/23758)).
- Cannot correctly check Table-per-Type classes. This is a limitation of EF Core.

The following are things I haven't bothered to check.

- Checking of Alternative keys
- Checking of sequences

## Introduction to how EfSchemaCompare works

EfSchemaCompare uses two EF Core features to get EF Core's internal schema and the database's schema. They are

1. EF Core's `Model` property in your application's DbContext. This provides the internal version of the database schema that EF Core builds from looking at the entity classes and any EF Core configuration attributes/methods.
2. EF Core's [Reverse Engineering](https://docs.microsoft.com/en-us/ef/core/managing-schemas/scaffolding) service, which allows me to access an actual database schema.

The fun part is comparing these two sources, especially with all the different types of configurations that EF Core can handle. The diagram shown below shows using EfSchemaCompare to check a test database that you updated with some SQL migration scripts against the current EF Core's `Model`.

![EfSchemaCompare diagram](https://github.com/JonPSmith/EfCore.SchemaCompare)

## How to use EfSchemaCompare

Here is a example of using the EfSchemaCompare feature

```c#
[Fact]
public void CompareViaContext()
{
    //SETUP
    using (var context = new BookContext(_options))
    {
        var comparer = new CompareEfSql();

        //ATTEMPT
        //This will compare EF Core model of the database with the database that the context's connection points to
        var hasErrors = comparer.CompareEfWithDb(context); 

        //VERIFY
        //The CompareEfWithDb method returns true if there were errors. 
        //The comparer.GetAllErrors property returns a string, with each error on a separate line
        hasErrors.ShouldBeFalse(comparer.GetAllErrors);
    }
}
```

### Different parameters to the `CompareEfWithDb` method

1. The `CompareEfWithDb` method can take multiple DbContexts, known as *bounded contexts* (see chapter 13, section 13.4.8 in the second edition of my book [Entity Framework Core in Action(https://bit.ly/EfCoreBookEd2)]). You can add as many contexts and they are compared to one database.
2. You can also provide a string that points to the database as the first parameter. It can have two forms:
   - It will use the string as a connection string name in the test's `appsetting.json` file.
   - If no connection string is found in the `appsetting.json` file it assumes it is a connection string.

See below for an example of both of of these options:
```c#
[Fact]
public void CompareBookThenOrderAgainstBookOrderDatabaseViaAppSettings()
{
    //SETUP
    //... I have left out how the options are created
    //This is the name of a connection string in the appsetting.json file in your test project
    const string connectionStringName = "BookOrderConnection";
    using (var context1 = new BookContext(options1))
    using (var context2 = new OrderContext(options2))
    {
        var comparer = new CompareEfSql();

        //ATTEMPT
        //Its starts with the connection string/name  and then you can have as many contexts as you like
        var hasErrors = comparer.CompareEfWithDb(connectionStringName, context1, context2);

        //VERIFY
        hasErrors.ShouldBeFalse(comparer.GetAllErrors);
    }
}
```

### Using with database provider not installed in `EfCore.SchemaCompare` library

The `EfCore.SchemaCompare` library only contains the SqlServer and Sqlite database providers. But if you want to run the compare with a specific database provider you can do that using the version with takes in a `IDesignTimeServices` for your database provider which contains the reverse engineering feature. For instance if you wanted to check a PostgreSql database you would use the following code shown below.

```c#
[Fact]
public void TestCompareEfSqlPostgreSql()
{
    //SETUP
    using (var context = new BookContext(_options))
    {
        var comparer = new CompareEfSql();

        //ATTEMPT
        //This will use the database provider design time type you gave to get the database information
        var hasErrors = comparer.CompareEfWithDb<NpgsqlDesignTimeServices>(context));

        //VERIFY
        hasErrors.ShouldBeFalse(comparer.GetAllErrors);
    }
}
```

## What to errors look like

The `comparer.GetAllErrors` property will return a string with each error separated by the `Environment.NewLine` string. Below is an example of an error

```text
"DIFFERENT: MyEntity->Property 'MyString'. Expected = varchar(max), found = nvarchar(max)"
```

The error above says

- `DIFFERENT:` There is a difference between EF Core and the database  (other settings are `NotInDatabase`, `ExtraInDatabase`)
- `MyEntity->Property 'MyString', column type` gives a description of what was checked
- `Expected = varchar(max)` says what EF Core thought it should be
- `found = nvarchar(max)` says what the database setting was

Here is another error coming from stage 2 where it checks the database side.

```text
EXTRA IN DATABASE: Table 'MyEntites', column name. Found = MyEntityId
```

This says that there is a column called `MyEntityId` in the table `MyEntites` that hasn't got a property in the entity class mapped to the `MyEntites` table.

## How to suppress errors

## Other configuration options

