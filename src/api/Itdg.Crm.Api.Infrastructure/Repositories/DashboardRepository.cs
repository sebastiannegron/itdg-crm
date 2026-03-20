namespace Itdg.Crm.Api.Infrastructure.Repositories;

using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Infrastructure.Data;

public class DashboardRepository : IDashboardRepository
{
    private readonly CrmDbContext _context;

    public DashboardRepository(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetTotalClientsCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Client>().CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<(ClientStatus Status, int Count)>> GetClientCountsByStatusAsync(CancellationToken cancellationToken = default)
    {
        var results = await _context.Set<Client>()
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return results
            .Select(r => (r.Status, r.Count))
            .ToList();
    }

    public async Task<IReadOnlyList<(Guid? TierId, string? TierName, int Count)>> GetClientCountsByTierAsync(CancellationToken cancellationToken = default)
    {
        var results = await _context.Set<Client>()
            .GroupBy(c => new { c.TierId, TierName = c.Tier != null ? c.Tier.Name : null })
            .Select(g => new { g.Key.TierId, g.Key.TierName, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return results
            .Select(r => (r.TierId, r.TierName, r.Count))
            .ToList();
    }

    public Task<int> GetPendingTasksCountAsync(CancellationToken cancellationToken = default)
    {
        // Task entity not yet implemented — will be updated when task management is added
        return Task.FromResult(0);
    }

    public async Task<int> GetRecentEscalationsCountAsync(CancellationToken cancellationToken = default)
    {
        var sevenDaysAgo = DateTimeOffset.UtcNow.AddDays(-7);

        return await _context.Set<Notification>()
            .Where(n => n.EventType == NotificationEventType.EscalationReceived
                && n.CreatedAt >= sevenDaysAgo)
            .CountAsync(cancellationToken);
    }

    public Task<int> GetUpcomingDeadlinesCountAsync(CancellationToken cancellationToken = default)
    {
        // Deadline entity not yet implemented — will be updated when task management is added
        return Task.FromResult(0);
    }

    public async Task<int> GetUnreadNotificationsCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Notification>()
            .Where(n => n.Status == NotificationStatus.Pending || n.Status == NotificationStatus.Delivered)
            .CountAsync(cancellationToken);
    }
}
