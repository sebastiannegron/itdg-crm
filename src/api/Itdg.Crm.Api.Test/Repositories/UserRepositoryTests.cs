namespace Itdg.Crm.Api.Test.Repositories;

using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Infrastructure.Repositories;

public class UserRepositoryTests
{
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
    }

    private TestDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    private class TestUserRepository : GenericRepository<User>
    {
        private readonly TestDbContext _testContext;

        public TestUserRepository(TestDbContext context) : base(context)
        {
            _testContext = context;
        }

        public async Task<User?> GetByEntraObjectIdAsync(string entraObjectId, CancellationToken cancellationToken = default)
        {
            return await _testContext.Users
                .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId, cancellationToken);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsUser_WhenUserExists()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestUserRepository(context);

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            EntraObjectId = "entra-object-id-1",
            Email = "test@example.com",
            DisplayName = "Test User",
            Role = UserRole.Administrator,
            IsActive = true
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.EntraObjectId.Should().Be("entra-object-id-1");
        result.Email.Should().Be("test@example.com");
        result.DisplayName.Should().Be("Test User");
        result.Role.Should().Be(UserRole.Administrator);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestUserRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEntraObjectIdAsync_ReturnsUser_WhenExists()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestUserRepository(context);

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            EntraObjectId = "entra-object-id-lookup",
            Email = "lookup@example.com",
            DisplayName = "Lookup User",
            Role = UserRole.Associate,
            IsActive = true
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByEntraObjectIdAsync("entra-object-id-lookup");

        // Assert
        result.Should().NotBeNull();
        result!.EntraObjectId.Should().Be("entra-object-id-lookup");
        result.Email.Should().Be("lookup@example.com");
    }

    [Fact]
    public async Task GetByEntraObjectIdAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestUserRepository(context);

        // Act
        var result = await repository.GetByEntraObjectIdAsync("non-existent-id");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_AddsUser_AndSavesChanges()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestUserRepository(context);

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            EntraObjectId = "new-entra-id",
            Email = "new@example.com",
            DisplayName = "New User",
            Role = UserRole.ClientPortal,
            IsActive = true
        };

        // Act
        await repository.AddAsync(user);

        // Assert
        var savedUser = await context.Users.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be("new@example.com");
        savedUser.DisplayName.Should().Be("New User");
        savedUser.Role.Should().Be(UserRole.ClientPortal);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesUser_AndSavesChanges()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestUserRepository(context);

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            EntraObjectId = "update-entra-id",
            Email = "original@example.com",
            DisplayName = "Original Name",
            Role = UserRole.Associate,
            IsActive = true
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act
        user.DisplayName = "Updated Name";
        user.Role = UserRole.Administrator;
        await repository.UpdateAsync(user);

        // Assert
        var updatedUser = await context.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.DisplayName.Should().Be("Updated Name");
        updatedUser.Role.Should().Be(UserRole.Administrator);
    }

    [Fact]
    public async Task DeleteAsync_RemovesUser_AndSavesChanges()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestUserRepository(context);

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            EntraObjectId = "delete-entra-id",
            Email = "delete@example.com",
            DisplayName = "Delete User",
            Role = UserRole.Associate,
            IsActive = true
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(user);

        // Assert
        var deletedUser = await context.Users.FindAsync(user.Id);
        deletedUser.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestUserRepository(context);
        var tenantId = Guid.NewGuid();

        var users = new[]
        {
            new User { Id = Guid.NewGuid(), TenantId = tenantId, EntraObjectId = "id-1", Email = "user1@example.com", DisplayName = "User 1", Role = UserRole.Administrator, IsActive = true },
            new User { Id = Guid.NewGuid(), TenantId = tenantId, EntraObjectId = "id-2", Email = "user2@example.com", DisplayName = "User 2", Role = UserRole.Associate, IsActive = true },
            new User { Id = Guid.NewGuid(), TenantId = tenantId, EntraObjectId = "id-3", Email = "user3@example.com", DisplayName = "User 3", Role = UserRole.ClientPortal, IsActive = false }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(u => u.DisplayName == "User 1");
        result.Should().Contain(u => u.DisplayName == "User 2");
        result.Should().Contain(u => u.DisplayName == "User 3");
    }
}
