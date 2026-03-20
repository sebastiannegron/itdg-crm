namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetNotificationsHandler : IQueryHandler<GetNotifications, PaginatedResultDto<NotificationDto>>
{
    private readonly INotificationRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetNotificationsHandler> _logger;

    public GetNotificationsHandler(
        INotificationRepository repository,
        IUserRepository userRepository,
        ICurrentUserProvider currentUserProvider,
        ILogger<GetNotificationsHandler> logger)
    {
        _repository = repository;
        _userRepository = userRepository;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task<PaginatedResultDto<NotificationDto>> HandleAsync(GetNotifications query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Notifications");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Getting notifications page {Page} | CorrelationId: {CorrelationId}", query.Page, correlationId);

        var userId = await ResolveUserIdAsync(cancellationToken);

        if (userId is null)
        {
            return new PaginatedResultDto<NotificationDto>(Items: [], TotalCount: 0, Page: query.Page, PageSize: query.PageSize);
        }

        var (items, totalCount) = await _repository.GetPagedByUserIdAsync(
            userId.Value,
            query.Page,
            query.PageSize,
            query.Status,
            cancellationToken);

        var dtos = items.Select(n => new NotificationDto(
            NotificationId: n.Id,
            UserId: n.UserId,
            EventType: n.EventType.ToString(),
            Channel: n.Channel.ToString(),
            Title: n.Title,
            Body: n.Body,
            Status: n.Status.ToString(),
            DeliveredAt: n.DeliveredAt,
            ReadAt: n.ReadAt,
            CreatedAt: n.CreatedAt
        )).ToList();

        return new PaginatedResultDto<NotificationDto>(
            Items: dtos,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize
        );
    }

    private async Task<Guid?> ResolveUserIdAsync(CancellationToken cancellationToken)
    {
        var entraObjectId = _currentUserProvider.GetEntraObjectId();
        if (string.IsNullOrWhiteSpace(entraObjectId))
        {
            return null;
        }

        var user = await _userRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);
        return user?.Id;
    }
}
