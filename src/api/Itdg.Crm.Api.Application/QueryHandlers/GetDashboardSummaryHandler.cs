namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetDashboardSummaryHandler : IQueryHandler<GetDashboardSummary, DashboardSummaryDto>
{
    private readonly IDashboardRepository _repository;
    private readonly ILogger<GetDashboardSummaryHandler> _logger;

    public GetDashboardSummaryHandler(
        IDashboardRepository repository,
        ILogger<GetDashboardSummaryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<DashboardSummaryDto> HandleAsync(GetDashboardSummary query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Dashboard Summary");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Getting dashboard summary | CorrelationId: {CorrelationId}", correlationId);

        var totalClients = await _repository.GetTotalClientsCountAsync(cancellationToken);
        var clientsByStatus = await _repository.GetClientCountsByStatusAsync(cancellationToken);
        var clientsByTier = await _repository.GetClientCountsByTierAsync(cancellationToken);
        var pendingTasksCount = await _repository.GetPendingTasksCountAsync(cancellationToken);
        var recentEscalationsCount = await _repository.GetRecentEscalationsCountAsync(cancellationToken);
        var upcomingDeadlinesCount = await _repository.GetUpcomingDeadlinesCountAsync(cancellationToken);
        var unreadNotificationsCount = await _repository.GetUnreadNotificationsCountAsync(cancellationToken);

        return new DashboardSummaryDto(
            TotalClients: totalClients,
            ClientsByStatus: clientsByStatus
                .Select(c => new ClientStatusCountDto(c.Status.ToString(), c.Count))
                .ToList(),
            ClientsByTier: clientsByTier
                .Select(c => new ClientTierCountDto(c.TierId, c.TierName, c.Count))
                .ToList(),
            PendingTasksCount: pendingTasksCount,
            RecentEscalationsCount: recentEscalationsCount,
            UpcomingDeadlinesCount: upcomingDeadlinesCount,
            UnreadNotificationsCount: unreadNotificationsCount
        );
    }
}
