namespace Itdg.Crm.Api.Test.Data;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Infrastructure.Data;

public class MessageConfigurationTests
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
    public void MessageConfiguration_HasCorrectTableName()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Message));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("Messages");
    }

    [Fact]
    public void MessageConfiguration_HasPrimaryKey()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Message));

        // Assert
        entityType.Should().NotBeNull();
        var primaryKey = entityType!.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().ContainSingle(p => p.Name == "Id");
    }

    [Fact]
    public void MessageConfiguration_SubjectProperty_HasMaxLength500()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Message));
        var subjectProperty = entityType!.FindProperty(nameof(Message.Subject));

        // Assert
        subjectProperty.Should().NotBeNull();
        subjectProperty!.IsNullable.Should().BeFalse();
        subjectProperty.GetMaxLength().Should().Be(500);
    }

    [Fact]
    public void MessageConfiguration_BodyProperty_IsRequired()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Message));
        var bodyProperty = entityType!.FindProperty(nameof(Message.Body));

        // Assert
        bodyProperty.Should().NotBeNull();
        bodyProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void MessageConfiguration_DirectionProperty_StoredAsString()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Message));
        var directionProperty = entityType!.FindProperty(nameof(Message.Direction));

        // Assert
        directionProperty.Should().NotBeNull();
        directionProperty!.GetProviderClrType().Should().Be(typeof(string));
    }

    [Fact]
    public void MessageConfiguration_HasTenantIdClientIdIndex()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Message));
        var indexes = entityType!.GetIndexes().ToList();

        // Assert
        var tenantClientIndex = indexes
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "TenantId") &&
                                 i.Properties.Any(p => p.Name == "ClientId") &&
                                 i.Properties.Count == 2);
        tenantClientIndex.Should().NotBeNull();
        tenantClientIndex!.GetDatabaseName().Should().Be("IX_Message_TenantId_ClientId");
    }

    [Fact]
    public void MessageConfiguration_HasTenantIdClientIdIsReadIndex()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Message));
        var indexes = entityType!.GetIndexes().ToList();

        // Assert
        var readIndex = indexes
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "TenantId") &&
                                 i.Properties.Any(p => p.Name == "ClientId") &&
                                 i.Properties.Any(p => p.Name == "IsRead"));
        readIndex.Should().NotBeNull();
        readIndex!.GetDatabaseName().Should().Be("IX_Message_TenantId_ClientId_IsRead");
    }

    [Fact]
    public async Task Message_CanBeSavedAndRetrieved()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.GetTenantId().Returns(tenantId);

        var options = new DbContextOptionsBuilder<TestCrmDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var message = new Message
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ClientId = Guid.NewGuid(),
            SenderId = Guid.NewGuid(),
            Direction = MessageDirection.Inbound,
            Subject = "Test Subject",
            Body = "Test body content",
            IsPortalMessage = true,
            IsRead = false
        };

        // Act
        using (var seedContext = new TestCrmDbContext(options, tenantProvider))
        {
            seedContext.Messages.Add(message);
            await seedContext.SaveChangesAsync();
        }

        using var queryContext = new TestCrmDbContext(options, tenantProvider);
        var result = await queryContext.Messages.FirstOrDefaultAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Subject.Should().Be("Test Subject");
        result.Body.Should().Be("Test body content");
        result.Direction.Should().Be(MessageDirection.Inbound);
        result.IsPortalMessage.Should().BeTrue();
        result.IsRead.Should().BeFalse();
    }

    [Fact]
    public void MessageDirection_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<MessageDirection>().Should().HaveCount(2);
        MessageDirection.Inbound.Should().BeDefined();
        MessageDirection.Outbound.Should().BeDefined();
    }
}
