namespace Itdg.Crm.Api.Application.Abstractions;

using Itdg.Crm.Api.Application.Dtos;

public interface INotificationHubContext
{
    Task SendNotificationAsync(Guid userId, NotificationDto notification, CancellationToken cancellationToken = default);
    Task SendUnreadCountAsync(Guid userId, int unreadCount, CancellationToken cancellationToken = default);
}
