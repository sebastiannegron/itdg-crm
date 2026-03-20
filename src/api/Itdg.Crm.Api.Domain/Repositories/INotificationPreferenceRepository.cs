namespace Itdg.Crm.Api.Domain.Repositories;

using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;

public interface INotificationPreferenceRepository : IGenericRepository<NotificationPreference>
{
    Task<IReadOnlyList<NotificationPreference>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationPreference>> GetByUserIdAndEventTypeAsync(Guid userId, NotificationEventType eventType, CancellationToken cancellationToken = default);
}
