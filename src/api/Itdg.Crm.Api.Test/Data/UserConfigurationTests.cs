namespace Itdg.Crm.Api.Test.Data;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Infrastructure.Data;

public class UserConfigurationTests
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
    public void UserConfiguration_HasCorrectTableName()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(User));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("Users");
    }

    [Fact]
    public void UserConfiguration_HasPrimaryKey()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(User));

        // Assert
        entityType.Should().NotBeNull();
        var primaryKey = entityType!.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().ContainSingle(p => p.Name == "Id");
    }

    [Fact]
    public void UserConfiguration_EntraObjectIdProperty_IsRequiredWithMaxLength128()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(User));
        var property = entityType!.FindProperty(nameof(User.EntraObjectId));

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(128);
    }

    [Fact]
    public void UserConfiguration_EmailProperty_IsRequiredWithMaxLength256()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(User));
        var property = entityType!.FindProperty(nameof(User.Email));

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(256);
    }

    [Fact]
    public void UserConfiguration_DisplayNameProperty_IsRequiredWithMaxLength256()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(User));
        var property = entityType!.FindProperty(nameof(User.DisplayName));

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(256);
    }

    [Fact]
    public void UserConfiguration_RoleProperty_StoredAsString()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(User));
        var roleProperty = entityType!.FindProperty(nameof(User.Role));

        // Assert
        roleProperty.Should().NotBeNull();
        roleProperty!.GetProviderClrType().Should().Be(typeof(string));
    }

    [Fact]
    public void UserConfiguration_HasUniqueEntraObjectIdIndex()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(User));
        var indexes = entityType!.GetIndexes().ToList();

        // Assert
        var entraObjectIdIndex = indexes
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "EntraObjectId"));
        entraObjectIdIndex.Should().NotBeNull();
        entraObjectIdIndex!.IsUnique.Should().BeTrue();
        entraObjectIdIndex.GetDatabaseName().Should().Be("IX_User_EntraObjectId");
    }

    [Fact]
    public void UserConfiguration_HasTenantIdEmailIndex()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(User));
        var indexes = entityType!.GetIndexes().ToList();

        // Assert
        var tenantEmailIndex = indexes
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "TenantId") &&
                                 i.Properties.Any(p => p.Name == "Email"));
        tenantEmailIndex.Should().NotBeNull();
        tenantEmailIndex!.GetDatabaseName().Should().Be("IX_User_TenantId_Email");
    }

    [Fact]
    public async Task User_CanBeSavedAndRetrieved()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.GetTenantId().Returns(tenantId);

        var options = new DbContextOptionsBuilder<TestCrmDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EntraObjectId = "00000000-0000-0000-0000-000000000001",
            Email = "test@example.com",
            DisplayName = "Test User",
            Role = UserRole.Administrator,
            IsActive = true
        };

        // Act
        using (var seedContext = new TestCrmDbContext(options, tenantProvider))
        {
            seedContext.Users.Add(user);
            await seedContext.SaveChangesAsync();
        }

        using var queryContext = new TestCrmDbContext(options, tenantProvider);
        var result = await queryContext.Users.FirstOrDefaultAsync();

        // Assert
        result.Should().NotBeNull();
        result!.EntraObjectId.Should().Be("00000000-0000-0000-0000-000000000001");
        result.Email.Should().Be("test@example.com");
        result.DisplayName.Should().Be("Test User");
        result.Role.Should().Be(UserRole.Administrator);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UserRole_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<UserRole>().Should().HaveCount(3);
        UserRole.Administrator.Should().BeDefined();
        UserRole.Associate.Should().BeDefined();
        UserRole.ClientPortal.Should().BeDefined();
    }

    [Fact]
    public void User_InheritsFromTenantEntity()
    {
        // Arrange & Act
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            EntraObjectId = "test-object-id",
            Email = "test@example.com",
            DisplayName = "Test User",
            Role = UserRole.Associate,
            IsActive = true
        };

        // Assert
        user.Should().BeAssignableTo<TenantEntity>();
        user.Should().BeAssignableTo<BaseEntity>();
        user.TenantId.Should().NotBeEmpty();
    }
}
