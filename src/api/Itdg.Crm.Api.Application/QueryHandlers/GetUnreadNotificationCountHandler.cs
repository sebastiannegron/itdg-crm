namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetUnreadNotificationCountHandler : IQueryHandler<GetUnreadNotificationCount, int>
{
    private readonly INotificationRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetUnreadNotificationCountHandler> _logger;

    public GetUnreadNotificationCountHandler(
        INotificationRepository repository,
        IUserRepository userRepository,
        ICurrentUserProvider currentUserProvider,
        ILogger<GetUnreadNotificationCountHandler> logger)
    {
        _repository = repository;
        _userRepository = userRepository;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task<int> HandleAsync(GetUnreadNotificationCount query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Unread Notification Count");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Getting unread notification count | CorrelationId: {CorrelationId}", correlationId);

        var entraObjectId = _currentUserProvider.GetEntraObjectId();
        if (string.IsNullOrWhiteSpace(entraObjectId))
        {
            return 0;
        }

        var user = await _userRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);
        if (user is null)
        {
            return 0;
        }

        return await _repository.GetUnreadCountByUserIdAsync(user.Id, cancellationToken);
    }
}
