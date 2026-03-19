namespace Itdg.Crm.Api.Test.Data;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Infrastructure.Data;

public class CrmDbContextQueryFilterTests
{
    private class TestTenantEntity : TenantEntity, ISoftDeletable
    {
        public required string Name { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }

    private class TestSoftDeletableEntity : BaseEntity, ISoftDeletable
    {
        public required string Name { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }

    /// <summary>
    /// Derived test context that registers test entity types in the model,
    /// inheriting all global query filter logic from CrmDbContext.
    /// </summary>
    private class TestCrmDbContext : CrmDbContext
    {
        public TestCrmDbContext(DbContextOptions<TestCrmDbContext> options, ITenantProvider tenantProvider)
            : base(options, tenantProvider)
        {
        }

        public DbSet<TestTenantEntity> TestTenantEntities { get; set; } = null!;
        public DbSet<TestSoftDeletableEntity> TestSoftDeletableEntities { get; set; } = null!;
    }

    private static TestCrmDbContext CreateContext(Guid tenantId, string databaseName)
    {
        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.GetTenantId().Returns(tenantId);

        var options = new DbContextOptionsBuilder<TestCrmDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new TestCrmDbContext(options, tenantProvider);
    }

    [Fact]
    public async Task QueryFilter_ExcludesSoftDeletedTenantEntities()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        // Seed data
        using (var seedContext = CreateContext(tenantId, dbName))
        {
            seedContext.TestTenantEntities.AddRange(
                new TestTenantEntity { Id = Guid.NewGuid(), Name = "Active", TenantId = tenantId, DeletedAt = null },
                new TestTenantEntity { Id = Guid.NewGuid(), Name = "Deleted", TenantId = tenantId, DeletedAt = DateTimeOffset.UtcNow }
            );
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantId, dbName);
        var results = await queryContext.TestTenantEntities.ToListAsync();

        // Assert
        results.Should().HaveCount(1);
        results[0].Name.Should().Be("Active");
    }

    [Fact]
    public async Task QueryFilter_ExcludesEntitiesFromOtherTenants()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        // Seed data with tenant A context
        using (var seedContext = CreateContext(tenantA, dbName))
        {
            seedContext.TestTenantEntities.AddRange(
                new TestTenantEntity { Id = Guid.NewGuid(), Name = "Tenant A Entity", TenantId = tenantA },
                new TestTenantEntity { Id = Guid.NewGuid(), Name = "Tenant B Entity", TenantId = tenantB }
            );
            await seedContext.SaveChangesAsync();
        }

        // Act — query with tenant A context
        using var queryContext = CreateContext(tenantA, dbName);
        var results = await queryContext.TestTenantEntities.ToListAsync();

        // Assert
        results.Should().HaveCount(1);
        results[0].Name.Should().Be("Tenant A Entity");
    }

    [Fact]
    public async Task QueryFilter_ExcludesSoftDeletedNonTenantEntities()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantId, dbName))
        {
            seedContext.TestSoftDeletableEntities.AddRange(
                new TestSoftDeletableEntity { Id = Guid.NewGuid(), Name = "Active" },
                new TestSoftDeletableEntity { Id = Guid.NewGuid(), Name = "Deleted", DeletedAt = DateTimeOffset.UtcNow }
            );
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantId, dbName);
        var results = await queryContext.TestSoftDeletableEntities.ToListAsync();

        // Assert
        results.Should().HaveCount(1);
        results[0].Name.Should().Be("Active");
    }

    [Fact]
    public async Task QueryFilter_CombinesTenantAndSoftDeleteFilters()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantA, dbName))
        {
            seedContext.TestTenantEntities.AddRange(
                new TestTenantEntity { Id = Guid.NewGuid(), Name = "A-Active", TenantId = tenantA },
                new TestTenantEntity { Id = Guid.NewGuid(), Name = "A-Deleted", TenantId = tenantA, DeletedAt = DateTimeOffset.UtcNow },
                new TestTenantEntity { Id = Guid.NewGuid(), Name = "B-Active", TenantId = tenantB },
                new TestTenantEntity { Id = Guid.NewGuid(), Name = "B-Deleted", TenantId = tenantB, DeletedAt = DateTimeOffset.UtcNow }
            );
            await seedContext.SaveChangesAsync();
        }

        // Act — query with tenant A context
        using var queryContext = CreateContext(tenantA, dbName);
        var results = await queryContext.TestTenantEntities.ToListAsync();

        // Assert — only active entities from tenant A
        results.Should().HaveCount(1);
        results[0].Name.Should().Be("A-Active");
    }

    [Fact]
    public async Task QueryFilter_IgnoreQueryFilters_ReturnsAllEntities()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantA, dbName))
        {
            seedContext.TestTenantEntities.AddRange(
                new TestTenantEntity { Id = Guid.NewGuid(), Name = "A-Active", TenantId = tenantA },
                new TestTenantEntity { Id = Guid.NewGuid(), Name = "A-Deleted", TenantId = tenantA, DeletedAt = DateTimeOffset.UtcNow },
                new TestTenantEntity { Id = Guid.NewGuid(), Name = "B-Active", TenantId = tenantB }
            );
            await seedContext.SaveChangesAsync();
        }

        // Act — query with filters disabled
        using var queryContext = CreateContext(tenantA, dbName);
        var results = await queryContext.TestTenantEntities.IgnoreQueryFilters().ToListAsync();

        // Assert — all entities returned regardless of tenant or soft-delete
        results.Should().HaveCount(3);
    }
}
