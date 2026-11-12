// Copyright (c) 2026 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using EfSchemaCompare;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests;

public class TestIndexOnComplexType(ITestOutputHelper output)
{
    public class Address
    {
        public string City { get; set; }
        public string ZipCode { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Address HomeAddress { get; set; }
    }

    public enum IndexConfigs { CityOnly, NameAndCity, CityOnlyRenamed, JsonNoIndex, JsonCity }

    public class ComplexIndexDbContext(DbContextOptions<ComplexIndexDbContext> options, IndexConfigs config)
        : DbContext(options)
    {
        public IndexConfigs Config { get; } = config;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var customer = modelBuilder.Entity<Customer>();
            customer.ComplexProperty(x => x.HomeAddress, b =>
            {
                b.IsRequired();
                if (Config is IndexConfigs.JsonNoIndex or IndexConfigs.JsonCity)
                    b.ToJson();
            });

            switch (Config)
            {
                case IndexConfigs.CityOnly:
                case IndexConfigs.JsonCity:
                    customer.HasIndex(x => x.HomeAddress.City);
                    break;
                case IndexConfigs.NameAndCity:
                    customer.HasIndex(x => new { x.Name, x.HomeAddress.City });
                    break;
                case IndexConfigs.CityOnlyRenamed:
                    customer.HasIndex(x => x.HomeAddress.City).HasDatabaseName("RenamedIndex");
                    break;
            }
        }
    }

    public class ConfigModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime)
            => context is ComplexIndexDbContext dynamicContext
                ? (context.GetType(), dynamicContext.Config, designTime)
                : (object)context.GetType();
    }

    private DbContextOptions<ComplexIndexDbContext> CreateOptions(string databaseSuffix) =>
        this.CreateUniqueMethodOptions<ComplexIndexDbContext>(
            builder => builder.ReplaceService<IModelCacheKeyFactory, ConfigModelCacheKeyFactory>(), databaseSuffix);

    private List<string> CompareDatabaseToModel(string databaseSuffix, IndexConfigs databaseConfig, IndexConfigs modelConfig)
    {
        var options = CreateOptions(databaseSuffix);
        using (var context = new ComplexIndexDbContext(options, databaseConfig))
        {
            context.Database.EnsureClean();
        }

        using (var context = new ComplexIndexDbContext(options, modelConfig))
        {
            var comparer = new CompareEfSql();
            comparer.CompareEfWithDb(context);
            var errors = CompareLog.ListAllErrors(comparer.Logs).ToList();
            foreach (var error in errors)
                output.WriteLine(error);
            return errors;
        }
    }

    [Theory]
    [InlineData(IndexConfigs.CityOnly)]
    [InlineData(IndexConfigs.NameAndCity)]
    public void IndexOnComplexTypePropertyMatchesDatabase(IndexConfigs config)
    {
        var errors = CompareDatabaseToModel(config.ToString(), config, config);

        errors.Where(x => x.Contains("Index")).ShouldBeEmpty();
    }

    [Fact]
    public void IndexOnComplexTypePropertyNotInDatabase()
    {
        var errors = CompareDatabaseToModel("Renamed", IndexConfigs.CityOnly, IndexConfigs.CityOnlyRenamed);

        errors.Where(x => x.Contains("Index")).ShouldEqual(new[]
        {
            "NOT IN DATABASE: Customer->Index 'HomeAddress_City', index constraint name. Expected = RenamedIndex"
        });
    }

    [Fact]
    public void IndexOnJsonMappedComplexTypePropertyNotInDatabase()
    {
        var errors = CompareDatabaseToModel("Json", IndexConfigs.JsonNoIndex, IndexConfigs.JsonCity);

        errors.Where(x => x.Contains("Index")).ShouldEqual(new[]
        {
            "NOT IN DATABASE: Customer->Index '', index constraint name. Expected = IX_Customer_HomeAddress_City"
        });
    }
}
