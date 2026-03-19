namespace Itdg.Crm.Api.Test.Data;

using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptorTests
{
    private class TestEntity : BaseEntity
    {
        public required string Name { get; set; }
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities => Set<TestEntity>();
    }

    private static TestDbContext CreateContext(AuditableEntityInterceptor interceptor)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        return new TestDbContext(options);
    }

    [Fact]
    public async Task SavingChangesAsync_SetsCreatedAtAndUpdatedAt_WhenEntityIsAdded()
    {
        // Arrange
        var interceptor = new AuditableEntityInterceptor();
        using var context = CreateContext(interceptor);

        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "New Entity" };

        // Act
        var before = DateTimeOffset.UtcNow;
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();
        var after = DateTimeOffset.UtcNow;

        // Assert
        entity.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        entity.UpdatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        entity.CreatedAt.Should().Be(entity.UpdatedAt);
    }

    [Fact]
    public async Task SavingChangesAsync_UpdatesOnlyUpdatedAt_WhenEntityIsModified()
    {
        // Arrange
        var interceptor = new AuditableEntityInterceptor();
        using var context = CreateContext(interceptor);

        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Original" };
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        var originalCreatedAt = entity.CreatedAt;

        // Act
        entity.Name = "Modified";
        context.Entry(entity).State = EntityState.Modified;
        await context.SaveChangesAsync();

        // Assert
        entity.CreatedAt.Should().Be(originalCreatedAt);
        entity.UpdatedAt.Should().BeOnOrAfter(originalCreatedAt);
    }

    [Fact]
    public void SavingChanges_SetsCreatedAtAndUpdatedAt_WhenEntityIsAdded()
    {
        // Arrange
        var interceptor = new AuditableEntityInterceptor();
        using var context = CreateContext(interceptor);

        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "New Entity" };

        // Act
        var before = DateTimeOffset.UtcNow;
        context.TestEntities.Add(entity);
        context.SaveChanges();
        var after = DateTimeOffset.UtcNow;

        // Assert
        entity.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        entity.UpdatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        entity.CreatedAt.Should().Be(entity.UpdatedAt);
    }

    [Fact]
    public void SavingChanges_UpdatesOnlyUpdatedAt_WhenEntityIsModified()
    {
        // Arrange
        var interceptor = new AuditableEntityInterceptor();
        using var context = CreateContext(interceptor);

        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Original" };
        context.TestEntities.Add(entity);
        context.SaveChanges();

        var originalCreatedAt = entity.CreatedAt;

        // Act
        entity.Name = "Modified";
        context.Entry(entity).State = EntityState.Modified;
        context.SaveChanges();

        // Assert
        entity.CreatedAt.Should().Be(originalCreatedAt);
        entity.UpdatedAt.Should().BeOnOrAfter(originalCreatedAt);
    }

    [Fact]
    public async Task SavingChangesAsync_DoesNotModifyTimestamps_WhenEntityIsDeleted()
    {
        // Arrange
        var interceptor = new AuditableEntityInterceptor();
        using var context = CreateContext(interceptor);

        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "To Delete" };
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        var originalCreatedAt = entity.CreatedAt;
        var originalUpdatedAt = entity.UpdatedAt;

        // Act
        context.TestEntities.Remove(entity);
        await context.SaveChangesAsync();

        // Assert
        entity.CreatedAt.Should().Be(originalCreatedAt);
        entity.UpdatedAt.Should().Be(originalUpdatedAt);
    }
}
