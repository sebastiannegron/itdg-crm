namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetNotificationPreferencesHandler : IQueryHandler<GetNotificationPreferences, IEnumerable<NotificationPreferenceDto>>
{
    private readonly INotificationPreferenceRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetNotificationPreferencesHandler> _logger;

    public GetNotificationPreferencesHandler(
        INotificationPreferenceRepository repository,
        IUserRepository userRepository,
        ICurrentUserProvider currentUserProvider,
        ILogger<GetNotificationPreferencesHandler> logger)
    {
        _repository = repository;
        _userRepository = userRepository;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task<IEnumerable<NotificationPreferenceDto>> HandleAsync(GetNotificationPreferences query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Notification Preferences");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Getting notification preferences | CorrelationId: {CorrelationId}", correlationId);

        var entraObjectId = _currentUserProvider.GetEntraObjectId();
        if (string.IsNullOrWhiteSpace(entraObjectId))
        {
            _logger.LogWarning("No Entra Object ID found for current user | CorrelationId: {CorrelationId}", correlationId);
            return [];
        }

        var user = await _userRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("User not found for EntraObjectId {EntraObjectId} | CorrelationId: {CorrelationId}", entraObjectId, correlationId);
            return [];
        }

        var preferences = await _repository.GetByUserIdAsync(user.Id, cancellationToken);

        return preferences.Select(p => new NotificationPreferenceDto(
            PreferenceId: p.Id,
            EventType: p.EventType.ToString(),
            Channel: p.Channel.ToString(),
            IsEnabled: p.IsEnabled,
            DigestMode: p.DigestMode
        )).ToList();
    }
}
