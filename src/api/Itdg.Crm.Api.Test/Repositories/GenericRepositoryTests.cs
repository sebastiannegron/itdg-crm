namespace Itdg.Crm.Api.Test.Repositories;

using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Infrastructure.Repositories;

public class GenericRepositoryTests
{
    // Test entity for repository operations
    private class TestEntity : BaseEntity
    {
        public required string Name { get; set; }
    }

    // Test DbContext with TestEntity included
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities => Set<TestEntity>();
    }

    private TestDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    // Test repository using the generic repository pattern
    private class TestRepository : GenericRepository<TestEntity>
    {
        public TestRepository(TestDbContext context) : base(context)
        {
        }
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEntity_WhenEntityExists()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestRepository(context);

        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test Entity" };
        await context.Set<TestEntity>().AddAsync(entity);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(entity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
        result.Name.Should().Be("Test Entity");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenEntityDoesNotExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities_WhenEntitiesExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestRepository(context);

        var entities = new[]
        {
            new TestEntity { Id = Guid.NewGuid(), Name = "Entity 1" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Entity 2" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Entity 3" }
        };

        await context.Set<TestEntity>().AddRangeAsync(entities);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(e => e.Name == "Entity 1");
        result.Should().Contain(e => e.Name == "Entity 2");
        result.Should().Contain(e => e.Name == "Entity 3");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoEntitiesExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_AddsEntity_AndSavesChanges()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestRepository(context);

        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "New Entity" };

        // Act
        await repository.AddAsync(entity);

        // Assert
        var savedEntity = await context.Set<TestEntity>().FindAsync(entity.Id);
        savedEntity.Should().NotBeNull();
        savedEntity!.Name.Should().Be("New Entity");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEntity_AndSavesChanges()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestRepository(context);

        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Original Name" };
        await context.Set<TestEntity>().AddAsync(entity);
        await context.SaveChangesAsync();

        // Act
        entity.Name = "Updated Name";
        await repository.UpdateAsync(entity);

        // Assert
        var updatedEntity = await context.Set<TestEntity>().FindAsync(entity.Id);
        updatedEntity.Should().NotBeNull();
        updatedEntity!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteAsync_RemovesEntity_AndSavesChanges()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestRepository(context);

        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "To Be Deleted" };
        await context.Set<TestEntity>().AddAsync(entity);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(entity);

        // Assert
        var deletedEntity = await context.Set<TestEntity>().FindAsync(entity.Id);
        deletedEntity.Should().BeNull();
    }
}
