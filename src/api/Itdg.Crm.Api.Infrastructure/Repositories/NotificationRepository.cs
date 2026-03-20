namespace Itdg.Crm.Api.Infrastructure.Repositories;

using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Infrastructure.Data;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Notification>()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> GetByUserIdAndStatusAsync(Guid userId, NotificationStatus status, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Notification>()
            .Where(n => n.UserId == userId && n.Status == status)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetPagedByUserIdAsync(Guid userId, int page, int pageSize, NotificationStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Notification>()
            .Where(n => n.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(n => n.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Notification>()
            .Where(n => n.UserId == userId && n.Status != NotificationStatus.Read)
            .CountAsync(cancellationToken);
    }

    public async Task MarkAllAsReadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await Context.Set<Notification>()
            .Where(n => n.UserId == userId && n.Status != NotificationStatus.Read)
            .ToListAsync(cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.Status = NotificationStatus.Read;
            notification.ReadAt = DateTimeOffset.UtcNow;
        }

        await Context.SaveChangesAsync(cancellationToken);
    }
}
