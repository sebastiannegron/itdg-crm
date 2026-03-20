namespace Itdg.Crm.Api.Domain.Repositories;

using Itdg.Crm.Api.Domain.GeneralConstants;

public interface IDashboardRepository
{
    Task<int> GetTotalClientsCountAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(ClientStatus Status, int Count)>> GetClientCountsByStatusAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(Guid? TierId, string? TierName, int Count)>> GetClientCountsByTierAsync(CancellationToken cancellationToken = default);
    Task<int> GetPendingTasksCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetRecentEscalationsCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetUpcomingDeadlinesCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetUnreadNotificationsCountAsync(CancellationToken cancellationToken = default);
}
