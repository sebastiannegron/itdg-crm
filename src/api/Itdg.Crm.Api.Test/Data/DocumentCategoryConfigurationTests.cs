namespace Itdg.Crm.Api.Test.Data;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Infrastructure.Data;
using Itdg.Crm.Api.Infrastructure.Data.Configurations;

public class DocumentCategoryConfigurationTests
{
    private class TestCrmDbContext : CrmDbContext
    {
        public TestCrmDbContext(DbContextOptions<TestCrmDbContext> options, ITenantProvider tenantProvider)
            : base(options, tenantProvider)
        {
        }
    }

    private static TestCrmDbContext CreateContext(string databaseName)
    {
        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.GetTenantId().Returns(Guid.NewGuid());

        var options = new DbContextOptionsBuilder<TestCrmDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new TestCrmDbContext(options, tenantProvider);
    }

    [Fact]
    public void DocumentCategoryConfiguration_HasCorrectTableName()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(DocumentCategory));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("DocumentCategories");
    }

    [Fact]
    public void DocumentCategoryConfiguration_HasPrimaryKey()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(DocumentCategory));

        // Assert
        entityType.Should().NotBeNull();
        var primaryKey = entityType!.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().ContainSingle(p => p.Name == "Id");
    }

    [Fact]
    public void DocumentCategoryConfiguration_NameProperty_IsRequired_HasMaxLength100()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(DocumentCategory));
        var nameProperty = entityType!.FindProperty(nameof(DocumentCategory.Name));

        // Assert
        nameProperty.Should().NotBeNull();
        nameProperty!.IsNullable.Should().BeFalse();
        nameProperty.GetMaxLength().Should().Be(100);
    }

    [Fact]
    public void DocumentCategoryConfiguration_NamingConventionProperty_HasMaxLength200()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(DocumentCategory));
        var property = entityType!.FindProperty(nameof(DocumentCategory.NamingConvention));

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeTrue();
        property.GetMaxLength().Should().Be(200);
    }

    [Fact]
    public void DocumentCategoryConfiguration_HasUniqueTenantIdNameIndex()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(DocumentCategory));
        var indexes = entityType!.GetIndexes().ToList();

        // Assert
        var index = indexes
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "TenantId") &&
                                  i.Properties.Any(p => p.Name == "Name") &&
                                  i.Properties.Count == 2);
        index.Should().NotBeNull();
        index!.IsUnique.Should().BeTrue();
        index.GetDatabaseName().Should().Be("IX_DocumentCategory_TenantId_Name");
    }

    [Fact]
    public void DocumentCategoryConfiguration_SeedsDefaultCategories()
    {
        // Arrange — build a model directly to access seed data
        var modelBuilder = new ModelBuilder();
        var configuration = new DocumentCategoryConfiguration();
        configuration.Configure(modelBuilder.Entity<DocumentCategory>());
        var model = modelBuilder.FinalizeModel();

        var entityType = model.FindEntityType(typeof(DocumentCategory));

        // Act
        var seedData = entityType!.GetSeedData().ToList();

        // Assert
        seedData.Should().HaveCount(6);

        var names = seedData.Select(s => s[nameof(DocumentCategory.Name)]!.ToString()).ToList();
        names.Should().Contain("Bank Statements");
        names.Should().Contain("Invoices");
        names.Should().Contain("Reports");
        names.Should().Contain("Tax Documents");
        names.Should().Contain("Contracts");
        names.Should().Contain("General");

        // All seeds should be marked as default
        foreach (var seed in seedData)
        {
            seed[nameof(DocumentCategory.IsDefault)].Should().Be(true);
            seed[nameof(DocumentCategory.TenantId)].Should().Be(TenantConfiguration.DefaultTenantId);
        }
    }

    [Fact]
    public async Task DocumentCategory_CanBeSavedAndRetrieved()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.GetTenantId().Returns(tenantId);

        var options = new DbContextOptionsBuilder<TestCrmDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var category = new DocumentCategory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Custom Category",
            NamingConvention = "{ClientName}_Custom_{Date}",
            IsDefault = false
        };

        // Act
        using (var seedContext = new TestCrmDbContext(options, tenantProvider))
        {
            seedContext.DocumentCategories.Add(category);
            await seedContext.SaveChangesAsync();
        }

        using var queryContext = new TestCrmDbContext(options, tenantProvider);
        var result = await queryContext.DocumentCategories.FirstOrDefaultAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Custom Category");
        result.NamingConvention.Should().Be("{ClientName}_Custom_{Date}");
        result.IsDefault.Should().BeFalse();
    }
}
