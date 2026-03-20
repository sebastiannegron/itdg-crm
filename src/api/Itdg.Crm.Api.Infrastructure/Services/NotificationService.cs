namespace Itdg.Crm.Api.Infrastructure.Services;

using System.Diagnostics;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Microsoft.Extensions.Logging;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _preferenceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository preferenceRepository,
        IUserRepository userRepository,
        IEmailSender emailSender,
        ITenantProvider tenantProvider,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _preferenceRepository = preferenceRepository;
        _userRepository = userRepository;
        _emailSender = emailSender;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task SendAsync(Guid userId, NotificationEventType eventType, string title, string body, IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Send Notification");
        activity?.SetTag("UserId", userId);
        activity?.SetTag("EventType", eventType.ToString());

        if (metadata is not null)
        {
            foreach (var (key, value) in metadata)
            {
                activity?.SetTag($"Metadata.{key}", value);
            }
        }

        _logger.LogInformation("Sending notification to user {UserId} for event {EventType}", userId, eventType);

        var enabledChannels = await ResolveEnabledChannelsAsync(userId, eventType, cancellationToken);

        foreach (var channel in enabledChannels)
        {
            try
            {
                switch (channel)
                {
                    case NotificationChannel.InApp:
                        await SendInAppAsync(userId, eventType, title, body, cancellationToken);
                        break;
                    case NotificationChannel.Email:
                        await SendEmailAsync(userId, title, body, cancellationToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification via {Channel} to user {UserId} for event {EventType}", channel, userId, eventType);
            }
        }
    }

    private async Task<IReadOnlyList<NotificationChannel>> ResolveEnabledChannelsAsync(Guid userId, NotificationEventType eventType, CancellationToken cancellationToken)
    {
        var preferences = await _preferenceRepository.GetByUserIdAndEventTypeAsync(userId, eventType, cancellationToken);

        if (preferences.Count == 0)
        {
            _logger.LogDebug("No notification preferences found for user {UserId} and event {EventType}, defaulting to all channels", userId, eventType);
            return [NotificationChannel.InApp, NotificationChannel.Email];
        }

        return preferences
            .Where(p => p.IsEnabled)
            .Select(p => p.Channel)
            .Distinct()
            .ToList();
    }

    private async Task SendInAppAsync(Guid userId, NotificationEventType eventType, string title, string body, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantProvider.GetTenantId(),
            UserId = userId,
            EventType = eventType,
            Channel = NotificationChannel.InApp,
            Title = title,
            Body = body,
            Status = NotificationStatus.Delivered,
            DeliveredAt = DateTimeOffset.UtcNow
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);

        _logger.LogInformation("In-app notification {NotificationId} delivered to user {UserId}", notification.Id, userId);
    }

    private async Task SendEmailAsync(Guid userId, string title, string body, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Cannot send email notification: user {UserId} not found", userId);
            return;
        }

        await _emailSender.SendAsync(user.Email, title, body, cancellationToken);

        _logger.LogInformation("Email notification sent to user {UserId} at {Email}", userId, user.Email);
    }
}
