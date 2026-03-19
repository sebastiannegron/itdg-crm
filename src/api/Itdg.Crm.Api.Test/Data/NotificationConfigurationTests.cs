namespace Itdg.Crm.Api.Test.Data;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Metadata;

public class NotificationConfigurationTests
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
    public void NotificationConfiguration_HasCorrectTableName()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Notification));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("Notifications");
    }

    [Fact]
    public void NotificationConfiguration_HasPrimaryKey()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Notification));

        // Assert
        entityType.Should().NotBeNull();
        var primaryKey = entityType!.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().ContainSingle(p => p.Name == "Id");
    }

    [Fact]
    public void NotificationConfiguration_TitleProperty_HasMaxLength256()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Notification));
        var titleProperty = entityType!.FindProperty(nameof(Notification.Title));

        // Assert
        titleProperty.Should().NotBeNull();
        titleProperty!.IsNullable.Should().BeFalse();
        titleProperty.GetMaxLength().Should().Be(256);
    }

    [Fact]
    public void NotificationConfiguration_BodyProperty_HasMaxLength4000()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Notification));
        var bodyProperty = entityType!.FindProperty(nameof(Notification.Body));

        // Assert
        bodyProperty.Should().NotBeNull();
        bodyProperty!.IsNullable.Should().BeFalse();
        bodyProperty.GetMaxLength().Should().Be(4000);
    }

    [Fact]
    public void NotificationConfiguration_EnumProperties_StoredAsStrings()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Notification));

        // Assert
        var eventTypeProperty = entityType!.FindProperty(nameof(Notification.EventType));
        eventTypeProperty.Should().NotBeNull();
        eventTypeProperty!.GetProviderClrType().Should().Be(typeof(string));

        var channelProperty = entityType.FindProperty(nameof(Notification.Channel));
        channelProperty.Should().NotBeNull();
        channelProperty!.GetProviderClrType().Should().Be(typeof(string));

        var statusProperty = entityType.FindProperty(nameof(Notification.Status));
        statusProperty.Should().NotBeNull();
        statusProperty!.GetProviderClrType().Should().Be(typeof(string));
    }

    [Fact]
    public void NotificationConfiguration_HasUserIdStatusIndex()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Notification));
        var indexes = entityType!.GetIndexes().ToList();

        // Assert
        var userIdStatusIndex = indexes
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "UserId") &&
                                 i.Properties.Any(p => p.Name == "Status"));
        userIdStatusIndex.Should().NotBeNull();
        userIdStatusIndex!.GetDatabaseName().Should().Be("IX_Notification_UserId_Status");
    }

    [Fact]
    public void NotificationPreferenceConfiguration_HasCorrectTableName()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(NotificationPreference));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("NotificationPreferences");
    }

    [Fact]
    public void NotificationPreferenceConfiguration_HasUniqueIndex()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(NotificationPreference));
        var indexes = entityType!.GetIndexes().ToList();

        // Assert
        var uniqueIndex = indexes
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "UserId") &&
                                 i.Properties.Any(p => p.Name == "EventType") &&
                                 i.Properties.Any(p => p.Name == "Channel"));
        uniqueIndex.Should().NotBeNull();
        uniqueIndex!.IsUnique.Should().BeTrue();
        uniqueIndex.GetDatabaseName().Should().Be("IX_NotificationPreference_UserId_EventType_Channel");
    }

    [Fact]
    public void NotificationPreferenceConfiguration_DigestModeProperty_HasMaxLength20()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(NotificationPreference));
        var digestProperty = entityType!.FindProperty(nameof(NotificationPreference.DigestMode));

        // Assert
        digestProperty.Should().NotBeNull();
        digestProperty!.IsNullable.Should().BeFalse();
        digestProperty.GetMaxLength().Should().Be(20);
    }

    [Fact]
    public async Task Notification_CanBeSavedAndRetrieved()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.GetTenantId().Returns(tenantId);

        var options = new DbContextOptionsBuilder<TestCrmDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = Guid.NewGuid(),
            EventType = NotificationEventType.DocumentUploaded,
            Channel = NotificationChannel.InApp,
            Title = "Test Notification",
            Body = "Test body content",
            Status = NotificationStatus.Pending
        };

        // Act
        using (var seedContext = new TestCrmDbContext(options, tenantProvider))
        {
            seedContext.Notifications.Add(notification);
            await seedContext.SaveChangesAsync();
        }

        using var queryContext = new TestCrmDbContext(options, tenantProvider);
        var result = await queryContext.Notifications.FirstOrDefaultAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Notification");
        result.EventType.Should().Be(NotificationEventType.DocumentUploaded);
        result.Channel.Should().Be(NotificationChannel.InApp);
        result.Status.Should().Be(NotificationStatus.Pending);
    }

    [Fact]
    public async Task NotificationPreference_CanBeSavedAndRetrieved()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.GetTenantId().Returns(tenantId);

        var options = new DbContextOptionsBuilder<TestCrmDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var preference = new NotificationPreference
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = Guid.NewGuid(),
            EventType = NotificationEventType.PaymentCompleted,
            Channel = NotificationChannel.Email,
            IsEnabled = true,
            DigestMode = "Immediate"
        };

        // Act
        using (var seedContext = new TestCrmDbContext(options, tenantProvider))
        {
            seedContext.NotificationPreferences.Add(preference);
            await seedContext.SaveChangesAsync();
        }

        using var queryContext = new TestCrmDbContext(options, tenantProvider);
        var result = await queryContext.NotificationPreferences.FirstOrDefaultAsync();

        // Assert
        result.Should().NotBeNull();
        result!.EventType.Should().Be(NotificationEventType.PaymentCompleted);
        result.Channel.Should().Be(NotificationChannel.Email);
        result.IsEnabled.Should().BeTrue();
        result.DigestMode.Should().Be("Immediate");
    }
}
