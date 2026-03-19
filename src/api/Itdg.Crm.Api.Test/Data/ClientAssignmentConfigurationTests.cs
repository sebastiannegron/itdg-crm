namespace Itdg.Crm.Api.Test.Data;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Infrastructure.Data;

public class ClientAssignmentConfigurationTests
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
    public void ClientAssignmentConfiguration_HasCorrectTableName()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(ClientAssignment));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("ClientAssignments");
    }

    [Fact]
    public void ClientAssignmentConfiguration_HasPrimaryKey()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(ClientAssignment));

        // Assert
        entityType.Should().NotBeNull();
        var primaryKey = entityType!.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().ContainSingle(p => p.Name == "Id");
    }

    [Fact]
    public void ClientAssignmentConfiguration_AssignedAtProperty_IsRequired()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(ClientAssignment));
        var property = entityType!.FindProperty(nameof(ClientAssignment.AssignedAt));

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void ClientAssignmentConfiguration_HasUniqueCompositeIndex()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(ClientAssignment));
        var indexes = entityType!.GetIndexes().ToList();

        // Assert
        var compositeIndex = indexes.FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "ClientId") &&
            i.Properties.Any(p => p.Name == "UserId"));
        compositeIndex.Should().NotBeNull();
        compositeIndex!.IsUnique.Should().BeTrue();
        compositeIndex.GetDatabaseName().Should().Be("IX_ClientAssignment_TenantId_ClientId_UserId");
    }

    [Fact]
    public void ClientAssignmentConfiguration_HasTenantIdUserIdIndex()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(ClientAssignment));
        var indexes = entityType!.GetIndexes().ToList();

        // Assert
        var userIdIndex = indexes.FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "UserId") &&
            i.Properties.Count == 2);
        userIdIndex.Should().NotBeNull();
        userIdIndex!.GetDatabaseName().Should().Be("IX_ClientAssignment_TenantId_UserId");
    }

    [Fact]
    public void ClientAssignmentConfiguration_HasClientForeignKey()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(ClientAssignment));
        var foreignKeys = entityType!.GetForeignKeys().ToList();

        // Assert
        var clientFk = foreignKeys.FirstOrDefault(fk =>
            fk.Properties.Any(p => p.Name == "ClientId"));
        clientFk.Should().NotBeNull();
        clientFk!.PrincipalEntityType.ClrType.Should().Be(typeof(Client));
    }

    [Fact]
    public void ClientAssignmentConfiguration_HasUserForeignKey()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(ClientAssignment));
        var foreignKeys = entityType!.GetForeignKeys().ToList();

        // Assert
        var userFk = foreignKeys.FirstOrDefault(fk =>
            fk.Properties.Any(p => p.Name == "UserId"));
        userFk.Should().NotBeNull();
        userFk!.PrincipalEntityType.ClrType.Should().Be(typeof(User));
    }

    [Fact]
    public void ClientAssignment_InheritsFromTenantEntity()
    {
        // Arrange & Act
        var assignment = new ClientAssignment
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AssignedAt = DateTimeOffset.UtcNow
        };

        // Assert
        assignment.Should().BeAssignableTo<TenantEntity>();
        assignment.Should().BeAssignableTo<BaseEntity>();
        assignment.TenantId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ClientAssignment_CanBeSavedAndRetrieved()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.GetTenantId().Returns(tenantId);

        var options = new DbContextOptionsBuilder<TestCrmDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var userId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var assignedAt = DateTimeOffset.UtcNow;

        // Seed a Client and User first (required for foreign keys in relational DB, but InMemory is relaxed)
        var assignment = new ClientAssignment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ClientId = clientId,
            UserId = userId,
            AssignedAt = assignedAt
        };

        // Act
        using (var seedContext = new TestCrmDbContext(options, tenantProvider))
        {
            seedContext.ClientAssignments.Add(assignment);
            await seedContext.SaveChangesAsync();
        }

        using var queryContext = new TestCrmDbContext(options, tenantProvider);
        var result = await queryContext.ClientAssignments.FirstOrDefaultAsync();

        // Assert
        result.Should().NotBeNull();
        result!.ClientId.Should().Be(clientId);
        result.UserId.Should().Be(userId);
        result.AssignedAt.Should().Be(assignedAt);
        result.TenantId.Should().Be(tenantId);
    }
}
