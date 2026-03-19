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
}
