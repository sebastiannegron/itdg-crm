namespace Itdg.Crm.Api.Application.Abstractions;

using Itdg.Crm.Api.Domain.GeneralConstants;

public interface INotificationService
{
    Task SendAsync(Guid userId, NotificationEventType eventType, string title, string body, IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);
}
