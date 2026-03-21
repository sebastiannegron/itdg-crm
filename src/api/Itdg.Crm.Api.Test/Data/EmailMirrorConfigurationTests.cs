namespace Itdg.Crm.Api.Test.Data;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Infrastructure.Data;

public class EmailMirrorConfigurationTests
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
    public void EmailMirrorConfiguration_HasCorrectTableName()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(EmailMirror));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("EmailMirrors");
    }

    [Fact]
    public void EmailMirrorConfiguration_HasPrimaryKey()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(EmailMirror));

        // Assert
        entityType.Should().NotBeNull();
        var primaryKey = entityType!.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().ContainSingle(p => p.Name == "Id");
    }

    [Fact]
    public void EmailMirrorConfiguration_GmailMessageIdProperty_HasMaxLength256()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(EmailMirror));
        var property = entityType!.FindProperty(nameof(EmailMirror.GmailMessageId));

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(256);
    }

    [Fact]
    public void EmailMirrorConfiguration_GmailThreadIdProperty_HasMaxLength256()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(EmailMirror));
        var property = entityType!.FindProperty(nameof(EmailMirror.GmailThreadId));

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(256);
    }

    [Fact]
    public void EmailMirrorConfiguration_SubjectProperty_HasMaxLength500()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(EmailMirror));
        var property = entityType!.FindProperty(nameof(EmailMirror.Subject));

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(500);
    }

    [Fact]
    public void EmailMirrorConfiguration_FromProperty_HasMaxLength256()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(EmailMirror));
        var property = entityType!.FindProperty(nameof(EmailMirror.From));

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(256);
    }

    [Fact]
    public void EmailMirrorConfiguration_ToProperty_HasMaxLength256()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(EmailMirror));
        var property = entityType!.FindProperty(nameof(EmailMirror.To));

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(256);
    }

    [Fact]
    public void EmailMirrorConfiguration_BodyPreviewProperty_IsNullable()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(EmailMirror));
        var property = entityType!.FindProperty(nameof(EmailMirror.BodyPreview));

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void EmailMirrorConfiguration_HasTenantIdClientIdReceivedAtIndex()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(EmailMirror));
        var indexes = entityType!.GetIndexes().ToList();

        // Assert
        var timelineIndex = indexes
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "TenantId") &&
                                 i.Properties.Any(p => p.Name == "ClientId") &&
                                 i.Properties.Any(p => p.Name == "ReceivedAt"));
        timelineIndex.Should().NotBeNull();
        timelineIndex!.GetDatabaseName().Should().Be("IX_EmailMirror_TenantId_ClientId_ReceivedAt");
    }

    [Fact]
    public void EmailMirrorConfiguration_HasUniqueGmailMessageIdIndex()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(EmailMirror));
        var indexes = entityType!.GetIndexes().ToList();

        // Assert
        var messageIdIndex = indexes
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "GmailMessageId") &&
                                 i.Properties.Count == 1);
        messageIdIndex.Should().NotBeNull();
        messageIdIndex!.IsUnique.Should().BeTrue();
        messageIdIndex.GetDatabaseName().Should().Be("IX_EmailMirror_GmailMessageId");
    }

    [Fact]
    public async Task EmailMirror_CanBeSavedAndRetrieved()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.GetTenantId().Returns(tenantId);

        var options = new DbContextOptionsBuilder<TestCrmDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var emailMirror = new EmailMirror
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ClientId = Guid.NewGuid(),
            GmailMessageId = "msg-123",
            GmailThreadId = "thread-456",
            Subject = "Test Subject",
            From = "sender@example.com",
            To = "recipient@example.com",
            BodyPreview = "Hello world preview",
            HasAttachments = true,
            ReceivedAt = DateTimeOffset.UtcNow
        };

        // Act
        using (var seedContext = new TestCrmDbContext(options, tenantProvider))
        {
            seedContext.EmailMirrors.Add(emailMirror);
            await seedContext.SaveChangesAsync();
        }

        using var queryContext = new TestCrmDbContext(options, tenantProvider);
        var result = await queryContext.EmailMirrors.FirstOrDefaultAsync();

        // Assert
        result.Should().NotBeNull();
        result!.GmailMessageId.Should().Be("msg-123");
        result.GmailThreadId.Should().Be("thread-456");
        result.Subject.Should().Be("Test Subject");
        result.From.Should().Be("sender@example.com");
        result.To.Should().Be("recipient@example.com");
        result.BodyPreview.Should().Be("Hello world preview");
        result.HasAttachments.Should().BeTrue();
    }
}
