namespace Itdg.Crm.Api.Infrastructure.Repositories;

using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Infrastructure.Data;

public class NotificationPreferenceRepository : GenericRepository<NotificationPreference>, INotificationPreferenceRepository
{
    public NotificationPreferenceRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<NotificationPreference>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<NotificationPreference>()
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationPreference>> GetByUserIdAndEventTypeAsync(Guid userId, NotificationEventType eventType, CancellationToken cancellationToken = default)
    {
        return await Context.Set<NotificationPreference>()
            .Where(p => p.UserId == userId && p.EventType == eventType)
            .ToListAsync(cancellationToken);
    }
}
