namespace Itdg.Crm.Api.Test.Services;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Infrastructure.Data;
using Itdg.Crm.Api.Infrastructure.Options;
using Itdg.Crm.Api.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class GmailSyncBackgroundServiceTests
{
    private class TestCrmDbContext : CrmDbContext
    {
        public TestCrmDbContext(DbContextOptions<TestCrmDbContext> options, ITenantProvider tenantProvider)
            : base(options, tenantProvider)
        {
        }
    }

    private readonly IServiceProvider _serviceProvider;
    private readonly IGmailService _gmailService;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<GmailSyncBackgroundService> _logger;
    private readonly Guid _tenantId = Guid.NewGuid();

    public GmailSyncBackgroundServiceTests()
    {
        _gmailService = Substitute.For<IGmailService>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _logger = Substitute.For<ILogger<GmailSyncBackgroundService>>();
        _serviceProvider = Substitute.For<IServiceProvider>();
    }

    private TestCrmDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<TestCrmDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new TestCrmDbContext(options, _tenantProvider);
    }

    private GmailSyncBackgroundService CreateService(GmailSyncOptions? syncOptions = null)
    {
        var options = Microsoft.Extensions.Options.Options.Create(
            syncOptions ?? new GmailSyncOptions());

        return new GmailSyncBackgroundService(_serviceProvider, options, _logger);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabled_DoesNotRun()
    {
        // Arrange
        var options = new GmailSyncOptions { Enabled = false };
        var service = CreateService(options);

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        await service.StopAsync(CancellationToken.None);

        // Assert — service should log that it's disabled and not call any services
        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("disabled")),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task SyncEmailsAsync_WhenNoClientsWithEmail_LogsAndReturns()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = CreateContext(dbName);

        var scopeServiceProvider = Substitute.For<IServiceProvider>();
        scopeServiceProvider.GetService(typeof(DbContext)).Returns(dbContext);
        scopeServiceProvider.GetService(typeof(IGmailService)).Returns(_gmailService);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(scopeServiceProvider);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        _serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(scopeFactory);

        var service = CreateService();

        // Act
        await service.SyncEmailsAsync(CancellationToken.None);

        // Assert
        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("0 clients")),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task SyncEmailsAsync_WithClientsAndNewEmails_StoresEmailMirrorRecords()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var clientId = Guid.NewGuid();
        var clientEmail = "client@example.com";

        // Seed a client
        using var seedContext = CreateContext(dbName);
        seedContext.Clients.Add(new Client
        {
            Id = clientId,
            TenantId = _tenantId,
            Name = "Test Client",
            ContactEmail = clientEmail
        });
        await seedContext.SaveChangesAsync();

        // Create a fresh context for the service to use
        using var dbContext = CreateContext(dbName);

        var scopeServiceProvider = Substitute.For<IServiceProvider>();
        scopeServiceProvider.GetService(typeof(DbContext)).Returns(dbContext);
        scopeServiceProvider.GetService(typeof(IGmailService)).Returns(_gmailService);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(scopeServiceProvider);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        _serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(scopeFactory);

        _gmailService.ListMessagesAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new GmailMessageListDto(
                [
                    new GmailMessageDto(
                        "msg-1", "thread-1", "Hello", "client@example.com",
                        "user@firm.com", "Snippet", "Body preview", false,
                        DateTimeOffset.UtcNow, [])
                ],
                null, 1));

        var service = CreateService();

        // Act
        await service.SyncEmailsAsync(CancellationToken.None);

        // Assert
        using var verifyContext = CreateContext(dbName);
        var mirrors = await verifyContext.Set<EmailMirror>().ToListAsync();
        mirrors.Should().HaveCount(1);
        mirrors[0].GmailMessageId.Should().Be("msg-1");
        mirrors[0].GmailThreadId.Should().Be("thread-1");
        mirrors[0].Subject.Should().Be("Hello");
        mirrors[0].From.Should().Be("client@example.com");
        mirrors[0].To.Should().Be("user@firm.com");
        mirrors[0].ClientId.Should().Be(clientId);
        mirrors[0].TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public async Task SyncEmailsAsync_SkipsAlreadySyncedMessages()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var clientId = Guid.NewGuid();
        var clientEmail = "client@example.com";

        // Seed a client and existing email mirror
        using var seedContext = CreateContext(dbName);
        seedContext.Clients.Add(new Client
        {
            Id = clientId,
            TenantId = _tenantId,
            Name = "Test Client",
            ContactEmail = clientEmail
        });
        seedContext.Set<EmailMirror>().Add(new EmailMirror
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            ClientId = clientId,
            GmailMessageId = "msg-existing",
            GmailThreadId = "thread-existing",
            Subject = "Existing",
            From = clientEmail,
            To = "user@firm.com",
            ReceivedAt = DateTimeOffset.UtcNow
        });
        await seedContext.SaveChangesAsync();

        using var dbContext = CreateContext(dbName);

        var scopeServiceProvider = Substitute.For<IServiceProvider>();
        scopeServiceProvider.GetService(typeof(DbContext)).Returns(dbContext);
        scopeServiceProvider.GetService(typeof(IGmailService)).Returns(_gmailService);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(scopeServiceProvider);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        _serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(scopeFactory);

        _gmailService.ListMessagesAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new GmailMessageListDto(
                [
                    new GmailMessageDto(
                        "msg-existing", "thread-existing", "Existing", clientEmail,
                        "user@firm.com", "Snippet", "Body", false,
                        DateTimeOffset.UtcNow, []),
                    new GmailMessageDto(
                        "msg-new", "thread-new", "New Email", clientEmail,
                        "user@firm.com", "Snippet", "Body", true,
                        DateTimeOffset.UtcNow, [])
                ],
                null, 2));

        var service = CreateService();

        // Act
        await service.SyncEmailsAsync(CancellationToken.None);

        // Assert — only the new one should be added
        using var verifyContext = CreateContext(dbName);
        var mirrors = await verifyContext.Set<EmailMirror>().ToListAsync();
        mirrors.Should().HaveCount(2);
        mirrors.Should().Contain(m => m.GmailMessageId == "msg-existing");
        mirrors.Should().Contain(m => m.GmailMessageId == "msg-new");
    }

    [Fact]
    public async Task SyncEmailsAsync_WhenGmailServiceFails_LogsWarningAndContinues()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var clientId = Guid.NewGuid();

        // Seed a client
        using var seedContext = CreateContext(dbName);
        seedContext.Clients.Add(new Client
        {
            Id = clientId,
            TenantId = _tenantId,
            Name = "Test Client",
            ContactEmail = "client@example.com"
        });
        await seedContext.SaveChangesAsync();

        using var dbContext = CreateContext(dbName);

        var scopeServiceProvider = Substitute.For<IServiceProvider>();
        scopeServiceProvider.GetService(typeof(DbContext)).Returns(dbContext);
        scopeServiceProvider.GetService(typeof(IGmailService)).Returns(_gmailService);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(scopeServiceProvider);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        _serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(scopeFactory);

        _gmailService.ListMessagesAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns<GmailMessageListDto>(x => throw new Exception("Gmail API error"));

        var service = CreateService();

        // Act — should not throw
        await service.SyncEmailsAsync(CancellationToken.None);

        // Assert — should log warning
        _logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed to sync")),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
