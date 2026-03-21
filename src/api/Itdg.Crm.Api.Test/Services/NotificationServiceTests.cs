namespace Itdg.Crm.Api.Test.Services;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Itdg.Crm.Api.Infrastructure.Services;
using Microsoft.Extensions.Logging;

public class NotificationServiceTests
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _preferenceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly ITenantProvider _tenantProvider;
    private readonly INotificationHubContext _hubContext;
    private readonly ILogger<NotificationService> _logger;
    private readonly NotificationService _service;

    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public NotificationServiceTests()
    {
        _notificationRepository = Substitute.For<INotificationRepository>();
        _preferenceRepository = Substitute.For<INotificationPreferenceRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _emailSender = Substitute.For<IEmailSender>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _hubContext = Substitute.For<INotificationHubContext>();
        _logger = Substitute.For<ILogger<NotificationService>>();

        _tenantProvider.GetTenantId().Returns(_tenantId);

        _service = new NotificationService(
            _notificationRepository,
            _preferenceRepository,
            _userRepository,
            _emailSender,
            _tenantProvider,
            _hubContext,
            _logger);
    }

    [Fact]
    public async Task SendAsync_NoPreferences_SendsToAllChannels()
    {
        // Arrange
        var user = new User
        {
            Id = _userId,
            TenantId = _tenantId,
            EntraObjectId = "entra-123",
            Email = "user@example.com",
            DisplayName = "Test User",
            Role = UserRole.Associate,
            IsActive = true
        };

        _preferenceRepository.GetByUserIdAndEventTypeAsync(_userId, NotificationEventType.TaskAssigned, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<NotificationPreference>());
        _userRepository.GetByIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(user);

        Notification? capturedNotification = null;
        _notificationRepository.AddAsync(Arg.Do<Notification>(n => capturedNotification = n), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _service.SendAsync(_userId, NotificationEventType.TaskAssigned, "Task Assigned", "You have a new task");

        // Assert
        await _notificationRepository.Received(1).AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
        await _emailSender.Received(1).SendAsync("user@example.com", "Task Assigned", "You have a new task", Arg.Any<CancellationToken>());

        capturedNotification.Should().NotBeNull();
        capturedNotification!.UserId.Should().Be(_userId);
        capturedNotification.TenantId.Should().Be(_tenantId);
        capturedNotification.EventType.Should().Be(NotificationEventType.TaskAssigned);
        capturedNotification.Channel.Should().Be(NotificationChannel.InApp);
        capturedNotification.Title.Should().Be("Task Assigned");
        capturedNotification.Body.Should().Be("You have a new task");
        capturedNotification.Status.Should().Be(NotificationStatus.Delivered);
        capturedNotification.DeliveredAt.Should().NotBeNull();
    }

    [Fact]
    public async Task SendAsync_InAppOnly_SendsOnlyInApp()
    {
        // Arrange
        var preferences = new List<NotificationPreference>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantId,
                UserId = _userId,
                EventType = NotificationEventType.PaymentCompleted,
                Channel = NotificationChannel.InApp,
                IsEnabled = true,
                DigestMode = "instant"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantId,
                UserId = _userId,
                EventType = NotificationEventType.PaymentCompleted,
                Channel = NotificationChannel.Email,
                IsEnabled = false,
                DigestMode = "instant"
            }
        };

        _preferenceRepository.GetByUserIdAndEventTypeAsync(_userId, NotificationEventType.PaymentCompleted, Arg.Any<CancellationToken>())
            .Returns(preferences);

        // Act
        await _service.SendAsync(_userId, NotificationEventType.PaymentCompleted, "Payment Completed", "Your payment was processed");

        // Assert
        await _notificationRepository.Received(1).AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
        await _emailSender.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_EmailOnly_SendsOnlyEmail()
    {
        // Arrange
        var user = new User
        {
            Id = _userId,
            TenantId = _tenantId,
            EntraObjectId = "entra-456",
            Email = "user@example.com",
            DisplayName = "Test User",
            Role = UserRole.Associate,
            IsActive = true
        };

        var preferences = new List<NotificationPreference>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantId,
                UserId = _userId,
                EventType = NotificationEventType.DocumentUploaded,
                Channel = NotificationChannel.InApp,
                IsEnabled = false,
                DigestMode = "instant"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantId,
                UserId = _userId,
                EventType = NotificationEventType.DocumentUploaded,
                Channel = NotificationChannel.Email,
                IsEnabled = true,
                DigestMode = "instant"
            }
        };

        _preferenceRepository.GetByUserIdAndEventTypeAsync(_userId, NotificationEventType.DocumentUploaded, Arg.Any<CancellationToken>())
            .Returns(preferences);
        _userRepository.GetByIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        await _service.SendAsync(_userId, NotificationEventType.DocumentUploaded, "Document Uploaded", "A new document was uploaded");

        // Assert
        await _notificationRepository.DidNotReceive().AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
        await _emailSender.Received(1).SendAsync("user@example.com", "Document Uploaded", "A new document was uploaded", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_AllChannelsDisabled_SendsNothing()
    {
        // Arrange
        var preferences = new List<NotificationPreference>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantId,
                UserId = _userId,
                EventType = NotificationEventType.SystemAlert,
                Channel = NotificationChannel.InApp,
                IsEnabled = false,
                DigestMode = "instant"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantId,
                UserId = _userId,
                EventType = NotificationEventType.SystemAlert,
                Channel = NotificationChannel.Email,
                IsEnabled = false,
                DigestMode = "instant"
            }
        };

        _preferenceRepository.GetByUserIdAndEventTypeAsync(_userId, NotificationEventType.SystemAlert, Arg.Any<CancellationToken>())
            .Returns(preferences);

        // Act
        await _service.SendAsync(_userId, NotificationEventType.SystemAlert, "System Alert", "System maintenance scheduled");

        // Assert
        await _notificationRepository.DidNotReceive().AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
        await _emailSender.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_EmailChannelUserNotFound_SkipsEmail()
    {
        // Arrange
        _preferenceRepository.GetByUserIdAndEventTypeAsync(_userId, NotificationEventType.TaskAssigned, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<NotificationPreference>());
        _userRepository.GetByIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        await _service.SendAsync(_userId, NotificationEventType.TaskAssigned, "Task Assigned", "You have a new task");

        // Assert — in-app still sent, email skipped
        await _notificationRepository.Received(1).AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
        await _emailSender.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_ChannelThrows_ContinuesWithOtherChannels()
    {
        // Arrange
        var user = new User
        {
            Id = _userId,
            TenantId = _tenantId,
            EntraObjectId = "entra-789",
            Email = "user@example.com",
            DisplayName = "Test User",
            Role = UserRole.Associate,
            IsActive = true
        };

        _preferenceRepository.GetByUserIdAndEventTypeAsync(_userId, NotificationEventType.EscalationReceived, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<NotificationPreference>());
        _userRepository.GetByIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(user);

        // InApp channel throws
        _notificationRepository.When(r => r.AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new InvalidOperationException("Database error"));

        // Act — should not throw
        var act = async () => await _service.SendAsync(_userId, NotificationEventType.EscalationReceived, "Escalation", "Case escalated");

        // Assert — continues to email despite in-app failure
        await act.Should().NotThrowAsync();
        await _emailSender.Received(1).SendAsync("user@example.com", "Escalation", "Case escalated", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_InAppNotification_SetsCorrectProperties()
    {
        // Arrange
        _preferenceRepository.GetByUserIdAndEventTypeAsync(_userId, NotificationEventType.PortalMessageReceived, Arg.Any<CancellationToken>())
            .Returns(new List<NotificationPreference>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    TenantId = _tenantId,
                    UserId = _userId,
                    EventType = NotificationEventType.PortalMessageReceived,
                    Channel = NotificationChannel.InApp,
                    IsEnabled = true,
                    DigestMode = "instant"
                }
            });

        Notification? captured = null;
        _notificationRepository.AddAsync(Arg.Do<Notification>(n => captured = n), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _service.SendAsync(_userId, NotificationEventType.PortalMessageReceived, "New Message", "You have a new portal message");

        // Assert
        captured.Should().NotBeNull();
        captured!.Id.Should().NotBe(Guid.Empty);
        captured.TenantId.Should().Be(_tenantId);
        captured.UserId.Should().Be(_userId);
        captured.EventType.Should().Be(NotificationEventType.PortalMessageReceived);
        captured.Channel.Should().Be(NotificationChannel.InApp);
        captured.Title.Should().Be("New Message");
        captured.Body.Should().Be("You have a new portal message");
        captured.Status.Should().Be(NotificationStatus.Delivered);
        captured.DeliveredAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SendAsync_InAppChannel_PushesNotificationViaSignalR()
    {
        // Arrange
        _preferenceRepository.GetByUserIdAndEventTypeAsync(_userId, NotificationEventType.TaskAssigned, Arg.Any<CancellationToken>())
            .Returns(new List<NotificationPreference>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    TenantId = _tenantId,
                    UserId = _userId,
                    EventType = NotificationEventType.TaskAssigned,
                    Channel = NotificationChannel.InApp,
                    IsEnabled = true,
                    DigestMode = "instant"
                }
            });

        _notificationRepository.GetUnreadCountByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(5);

        // Act
        await _service.SendAsync(_userId, NotificationEventType.TaskAssigned, "Task Assigned", "You have a new task");

        // Assert
        await _hubContext.Received(1).SendNotificationAsync(
            _userId,
            Arg.Is<NotificationDto>(dto =>
                dto.Title == "Task Assigned" &&
                dto.Body == "You have a new task" &&
                dto.EventType == "TaskAssigned" &&
                dto.Channel == "InApp"),
            Arg.Any<CancellationToken>());

        await _hubContext.Received(1).SendUnreadCountAsync(
            _userId,
            5,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_InAppDisabled_DoesNotPushViaSignalR()
    {
        // Arrange
        var user = new User
        {
            Id = _userId,
            TenantId = _tenantId,
            EntraObjectId = "entra-signalr",
            Email = "user@example.com",
            DisplayName = "Test User",
            Role = UserRole.Associate,
            IsActive = true
        };

        var preferences = new List<NotificationPreference>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantId,
                UserId = _userId,
                EventType = NotificationEventType.DocumentUploaded,
                Channel = NotificationChannel.InApp,
                IsEnabled = false,
                DigestMode = "instant"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantId,
                UserId = _userId,
                EventType = NotificationEventType.DocumentUploaded,
                Channel = NotificationChannel.Email,
                IsEnabled = true,
                DigestMode = "instant"
            }
        };

        _preferenceRepository.GetByUserIdAndEventTypeAsync(_userId, NotificationEventType.DocumentUploaded, Arg.Any<CancellationToken>())
            .Returns(preferences);
        _userRepository.GetByIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        await _service.SendAsync(_userId, NotificationEventType.DocumentUploaded, "Document Uploaded", "A new document was uploaded");

        // Assert — no SignalR push since in-app is disabled
        await _hubContext.DidNotReceive().SendNotificationAsync(Arg.Any<Guid>(), Arg.Any<NotificationDto>(), Arg.Any<CancellationToken>());
        await _hubContext.DidNotReceive().SendUnreadCountAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_SignalRFailure_DoesNotThrow()
    {
        // Arrange
        _preferenceRepository.GetByUserIdAndEventTypeAsync(_userId, NotificationEventType.TaskAssigned, Arg.Any<CancellationToken>())
            .Returns(new List<NotificationPreference>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    TenantId = _tenantId,
                    UserId = _userId,
                    EventType = NotificationEventType.TaskAssigned,
                    Channel = NotificationChannel.InApp,
                    IsEnabled = true,
                    DigestMode = "instant"
                }
            });

        _hubContext.SendNotificationAsync(Arg.Any<Guid>(), Arg.Any<NotificationDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("SignalR connection lost")));

        // Act — should not throw even when SignalR fails
        var act = async () => await _service.SendAsync(_userId, NotificationEventType.TaskAssigned, "Task Assigned", "You have a new task");

        // Assert
        await act.Should().NotThrowAsync();
        await _notificationRepository.Received(1).AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
    }
}
