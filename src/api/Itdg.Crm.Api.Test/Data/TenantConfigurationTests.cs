namespace Itdg.Crm.Api.Test.Data;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Infrastructure.Data;
using Itdg.Crm.Api.Infrastructure.Data.Configurations;

public class TenantConfigurationTests
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
        tenantProvider.GetTenantId().Returns(Guid.Empty);

        var options = new DbContextOptionsBuilder<TestCrmDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new TestCrmDbContext(options, tenantProvider);
    }

    [Fact]
    public void TenantEntity_HasExpectedProperties()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Tenant",
            Subdomain = "test",
            Settings = "{\"theme\":\"dark\"}"
        };

        // Assert
        tenant.Name.Should().Be("Test Tenant");
        tenant.Subdomain.Should().Be("test");
        tenant.Settings.Should().Be("{\"theme\":\"dark\"}");
    }

    [Fact]
    public void TenantEntity_InheritsFromBaseEntity()
    {
        // Arrange & Act
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Subdomain = "test"
        };

        // Assert
        tenant.Should().BeAssignableTo<BaseEntity>();
        tenant.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void TenantEntity_SettingsIsNullable()
    {
        // Arrange & Act
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Subdomain = "test",
            Settings = null
        };

        // Assert
        tenant.Settings.Should().BeNull();
    }

    [Fact]
    public async Task TenantConfiguration_PersistsAndRetrievesTenant()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var tenantId = Guid.NewGuid();

        using (var context = CreateContext(dbName))
        {
            context.Tenants.Add(new Tenant
            {
                Id = tenantId,
                Name = "Acme Corp",
                Subdomain = "acme",
                Settings = "{\"primaryColor\":\"#E85320\"}"
            });
            await context.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(dbName);
        var tenant = await queryContext.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tenantId);

        // Assert
        tenant.Should().NotBeNull();
        tenant!.Name.Should().Be("Acme Corp");
        tenant.Subdomain.Should().Be("acme");
        tenant.Settings.Should().Be("{\"primaryColor\":\"#E85320\"}");
    }

    [Fact]
    public void TenantConfiguration_SeedsDefaultDevelopmentTenant()
    {
        // Arrange — build a model directly to access seed data
        var modelBuilder = new ModelBuilder();
        var configuration = new TenantConfiguration();
        configuration.Configure(modelBuilder.Entity<Tenant>());
        var model = modelBuilder.FinalizeModel();

        var entityType = model.FindEntityType(typeof(Tenant));

        // Act
        var seedData = entityType!.GetSeedData().ToList();

        // Assert
        seedData.Should().HaveCount(1);

        var seed = seedData[0];
        seed[nameof(Tenant.Id)].Should().Be(TenantConfiguration.DefaultTenantId);
        seed[nameof(Tenant.Name)].Should().Be("Development Tenant");
        seed[nameof(Tenant.Subdomain)].Should().Be("dev");
    }

    [Fact]
    public void TenantConfiguration_SubdomainIsRequired()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();

        using var context = CreateContext(dbName);

        // The InMemoryDatabase doesn't enforce constraints the same as SQL Server,
        // but we verify the model configuration is correct
        var entityType = context.Model.FindEntityType(typeof(Tenant));

        // Assert
        var subdomainProperty = entityType!.FindProperty(nameof(Tenant.Subdomain));
        subdomainProperty.Should().NotBeNull();
        subdomainProperty!.IsNullable.Should().BeFalse();
        subdomainProperty.GetMaxLength().Should().Be(128);
    }

    [Fact]
    public void TenantConfiguration_NameIsRequired()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();

        using var context = CreateContext(dbName);
        var entityType = context.Model.FindEntityType(typeof(Tenant));

        // Assert
        var nameProperty = entityType!.FindProperty(nameof(Tenant.Name));
        nameProperty.Should().NotBeNull();
        nameProperty!.IsNullable.Should().BeFalse();
        nameProperty.GetMaxLength().Should().Be(256);
    }

    [Fact]
    public void TenantConfiguration_HasUniqueSubdomainIndex()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();

        using var context = CreateContext(dbName);
        var entityType = context.Model.FindEntityType(typeof(Tenant));

        // Assert
        var index = entityType!.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(Tenant.Subdomain)));
        index.Should().NotBeNull();
        index!.IsUnique.Should().BeTrue();
    }

    [Fact]
    public void TenantConfiguration_TableNameIsTenants()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();

        using var context = CreateContext(dbName);
        var entityType = context.Model.FindEntityType(typeof(Tenant));

        // Assert
        entityType!.GetTableName().Should().Be("Tenants");
    }
}
