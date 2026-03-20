namespace Itdg.Crm.Api.Test.Repositories;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Infrastructure.Data;
using Itdg.Crm.Api.Infrastructure.Repositories;

public class MessageRepositoryTests
{
    private class TestCrmDbContext : CrmDbContext
    {
        public TestCrmDbContext(DbContextOptions<TestCrmDbContext> options, ITenantProvider tenantProvider)
            : base(options, tenantProvider)
        {
        }
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

    private static Message CreateMessage(
        Guid tenantId,
        Guid clientId,
        MessageDirection direction = MessageDirection.Inbound,
        string subject = "Test Subject",
        bool isRead = false)
    {
        return new Message
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ClientId = clientId,
            SenderId = Guid.NewGuid(),
            Direction = direction,
            Subject = subject,
            Body = "Test body content",
            IsPortalMessage = true,
            IsRead = isRead
        };
    }

    [Fact]
    public async Task GetByClientIdAsync_ReturnsMessagesForClient()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var otherClientId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantId, dbName))
        {
            seedContext.Messages.AddRange(
                CreateMessage(tenantId, clientId, subject: "Client 1 Message 1"),
                CreateMessage(tenantId, clientId, subject: "Client 1 Message 2"),
                CreateMessage(tenantId, otherClientId, subject: "Other Client Message")
            );
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantId, dbName);
        var repository = new MessageRepository(queryContext);
        var results = await repository.GetByClientIdAsync(clientId);

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(m => m.ClientId.Should().Be(clientId));
    }

    [Fact]
    public async Task GetByClientIdAsync_ReturnsEmpty_WhenNoMessages()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantId, dbName))
        {
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantId, dbName);
        var repository = new MessageRepository(queryContext);
        var results = await repository.GetByClientIdAsync(clientId);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByClientIdAsync_ReturnsOrderedByCreatedAtDescending()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantId, dbName))
        {
            var older = CreateMessage(tenantId, clientId, subject: "Older");
            older.CreatedAt = DateTimeOffset.UtcNow.AddDays(-1);

            var newer = CreateMessage(tenantId, clientId, subject: "Newer");
            newer.CreatedAt = DateTimeOffset.UtcNow;

            seedContext.Messages.AddRange(older, newer);
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantId, dbName);
        var repository = new MessageRepository(queryContext);
        var results = await repository.GetByClientIdAsync(clientId);

        // Assert
        results.Should().HaveCount(2);
        results[0].Subject.Should().Be("Newer");
        results[1].Subject.Should().Be("Older");
    }

    [Fact]
    public async Task GetByIdAndClientIdAsync_ReturnsMessage_WhenExists()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();
        var messageId = Guid.NewGuid();

        using (var seedContext = CreateContext(tenantId, dbName))
        {
            var message = CreateMessage(tenantId, clientId, subject: "Found");
            message.Id = messageId;
            seedContext.Messages.Add(message);
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantId, dbName);
        var repository = new MessageRepository(queryContext);
        var result = await repository.GetByIdAndClientIdAsync(messageId, clientId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(messageId);
        result.ClientId.Should().Be(clientId);
        result.Subject.Should().Be("Found");
    }

    [Fact]
    public async Task GetByIdAndClientIdAsync_ReturnsNull_WhenMessageDoesNotBelongToClient()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var otherClientId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();
        var messageId = Guid.NewGuid();

        using (var seedContext = CreateContext(tenantId, dbName))
        {
            var message = CreateMessage(tenantId, otherClientId, subject: "Other Client");
            message.Id = messageId;
            seedContext.Messages.Add(message);
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantId, dbName);
        var repository = new MessageRepository(queryContext);
        var result = await repository.GetByIdAndClientIdAsync(messageId, clientId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAndClientIdAsync_ReturnsNull_WhenMessageDoesNotExist()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantId, dbName))
        {
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantId, dbName);
        var repository = new MessageRepository(queryContext);
        var result = await repository.GetByIdAndClientIdAsync(Guid.NewGuid(), clientId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task TenantFilter_IsolatesMessagesByTenant()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantA, dbName))
        {
            seedContext.Messages.AddRange(
                CreateMessage(tenantA, clientId, subject: "Tenant A Message"),
                CreateMessage(tenantB, clientId, subject: "Tenant B Message")
            );
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantA, dbName);
        var repository = new MessageRepository(queryContext);
        var results = await repository.GetAllAsync();

        // Assert
        results.Should().HaveCount(1);
        results[0].Subject.Should().Be("Tenant A Message");
    }

    [Fact]
    public async Task SoftDeleteFilter_ExcludesDeletedMessages()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantId, dbName))
        {
            var active = CreateMessage(tenantId, clientId, subject: "Active");
            var deleted = CreateMessage(tenantId, clientId, subject: "Deleted");
            deleted.DeletedAt = DateTimeOffset.UtcNow;

            seedContext.Messages.AddRange(active, deleted);
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantId, dbName);
        var repository = new MessageRepository(queryContext);
        var results = await repository.GetAllAsync();

        // Assert
        results.Should().HaveCount(1);
        results[0].Subject.Should().Be("Active");
    }
}
