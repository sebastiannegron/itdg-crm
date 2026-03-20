namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class MarkAllNotificationsAsReadHandler : ICommandHandler<MarkAllNotificationsAsRead>
{
    private readonly INotificationRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<MarkAllNotificationsAsReadHandler> _logger;

    public MarkAllNotificationsAsReadHandler(
        INotificationRepository repository,
        IUserRepository userRepository,
        ICurrentUserProvider currentUserProvider,
        ILogger<MarkAllNotificationsAsReadHandler> logger)
    {
        _repository = repository;
        _userRepository = userRepository;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task HandleAsync(MarkAllNotificationsAsRead command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Mark All Notifications As Read");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Marking all notifications as read | CorrelationId: {CorrelationId}", correlationId);

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

        await _repository.MarkAllAsReadByUserIdAsync(user.Id, cancellationToken);

        _logger.LogInformation("All notifications marked as read for user {UserId} | CorrelationId: {CorrelationId}", user.Id, correlationId);
    }
}
