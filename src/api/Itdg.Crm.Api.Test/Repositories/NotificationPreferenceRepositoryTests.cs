namespace Itdg.Crm.Api.Test.Repositories;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Infrastructure.Data;
using Itdg.Crm.Api.Infrastructure.Repositories;

public class NotificationPreferenceRepositoryTests
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

    private static NotificationPreference CreatePreference(
        Guid tenantId,
        Guid userId,
        NotificationEventType eventType = NotificationEventType.TaskAssigned,
        NotificationChannel channel = NotificationChannel.InApp,
        bool isEnabled = true)
    {
        return new NotificationPreference
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            EventType = eventType,
            Channel = channel,
            IsEnabled = isEnabled,
            DigestMode = "instant"
        };
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsPreferencesForUser()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantId, dbName))
        {
            seedContext.NotificationPreferences.AddRange(
                CreatePreference(tenantId, userId, NotificationEventType.TaskAssigned, NotificationChannel.InApp),
                CreatePreference(tenantId, userId, NotificationEventType.TaskAssigned, NotificationChannel.Email),
                CreatePreference(tenantId, otherUserId, NotificationEventType.TaskAssigned, NotificationChannel.InApp)
            );
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantId, dbName);
        var repository = new NotificationPreferenceRepository(queryContext);
        var results = await repository.GetByUserIdAsync(userId);

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(p => p.UserId.Should().Be(userId));
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsEmpty_WhenNoPreferences()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantId, dbName))
        {
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantId, dbName);
        var repository = new NotificationPreferenceRepository(queryContext);
        var results = await repository.GetByUserIdAsync(userId);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAndEventTypeAsync_ReturnsFilteredPreferences()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantId, dbName))
        {
            seedContext.NotificationPreferences.AddRange(
                CreatePreference(tenantId, userId, NotificationEventType.TaskAssigned, NotificationChannel.InApp),
                CreatePreference(tenantId, userId, NotificationEventType.TaskAssigned, NotificationChannel.Email),
                CreatePreference(tenantId, userId, NotificationEventType.PaymentCompleted, NotificationChannel.InApp)
            );
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantId, dbName);
        var repository = new NotificationPreferenceRepository(queryContext);
        var results = await repository.GetByUserIdAndEventTypeAsync(userId, NotificationEventType.TaskAssigned);

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(p =>
        {
            p.UserId.Should().Be(userId);
            p.EventType.Should().Be(NotificationEventType.TaskAssigned);
        });
    }

    [Fact]
    public async Task GetByUserIdAndEventTypeAsync_ReturnsEmpty_WhenNoMatchingPreferences()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantId, dbName))
        {
            seedContext.NotificationPreferences.Add(
                CreatePreference(tenantId, userId, NotificationEventType.PaymentCompleted, NotificationChannel.InApp)
            );
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantId, dbName);
        var repository = new NotificationPreferenceRepository(queryContext);
        var results = await repository.GetByUserIdAndEventTypeAsync(userId, NotificationEventType.TaskAssigned);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task TenantFilter_IsolatesPreferencesByTenant()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantA, dbName))
        {
            seedContext.NotificationPreferences.AddRange(
                CreatePreference(tenantA, userId, NotificationEventType.TaskAssigned, NotificationChannel.InApp),
                CreatePreference(tenantB, userId, NotificationEventType.TaskAssigned, NotificationChannel.Email)
            );
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantA, dbName);
        var repository = new NotificationPreferenceRepository(queryContext);
        var results = await repository.GetAllAsync();

        // Assert
        results.Should().HaveCount(1);
        results[0].TenantId.Should().Be(tenantA);
    }
}
