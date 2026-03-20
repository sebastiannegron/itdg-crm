namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class UpdateNotificationPreferencesHandler : ICommandHandler<UpdateNotificationPreferences>
{
    private readonly INotificationPreferenceRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<UpdateNotificationPreferencesHandler> _logger;

    public UpdateNotificationPreferencesHandler(
        INotificationPreferenceRepository repository,
        IUserRepository userRepository,
        ICurrentUserProvider currentUserProvider,
        ITenantProvider tenantProvider,
        ILogger<UpdateNotificationPreferencesHandler> logger)
    {
        _repository = repository;
        _userRepository = userRepository;
        _currentUserProvider = currentUserProvider;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateNotificationPreferences command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Update Notification Preferences");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Updating notification preferences | CorrelationId: {CorrelationId}", correlationId);

        var entraObjectId = _currentUserProvider.GetEntraObjectId();
        if (string.IsNullOrWhiteSpace(entraObjectId))
        {
            _logger.LogWarning("No Entra Object ID found for current user | CorrelationId: {CorrelationId}", correlationId);
            return;
        }

        var user = await _userRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("User not found for EntraObjectId {EntraObjectId} | CorrelationId: {CorrelationId}", entraObjectId, correlationId);
            return;
        }

        var existingPreferences = await _repository.GetByUserIdAsync(user.Id, cancellationToken);

        foreach (var dto in command.Preferences)
        {
            if (!Enum.TryParse<NotificationEventType>(dto.EventType, out var eventType) ||
                !Enum.TryParse<NotificationChannel>(dto.Channel, out var channel))
            {
                _logger.LogWarning("Invalid event type {EventType} or channel {Channel} | CorrelationId: {CorrelationId}", dto.EventType, dto.Channel, correlationId);
                continue;
            }

            var existing = existingPreferences.FirstOrDefault(p => p.EventType == eventType && p.Channel == channel);

            if (existing is not null)
            {
                existing.IsEnabled = dto.IsEnabled;
                existing.DigestMode = dto.DigestMode;
                await _repository.UpdateAsync(existing, cancellationToken);
            }
            else
            {
                var preference = new NotificationPreference
                {
                    Id = Guid.NewGuid(),
                    TenantId = _tenantProvider.GetTenantId(),
                    UserId = user.Id,
                    EventType = eventType,
                    Channel = channel,
                    IsEnabled = dto.IsEnabled,
                    DigestMode = dto.DigestMode
                };
                await _repository.AddAsync(preference, cancellationToken);
            }
        }

        _logger.LogInformation("Notification preferences updated for user {UserId} | CorrelationId: {CorrelationId}", user.Id, correlationId);
    }
}
