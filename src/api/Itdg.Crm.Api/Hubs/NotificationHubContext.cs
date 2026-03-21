namespace Itdg.Crm.Api.Hubs;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Microsoft.AspNetCore.SignalR;

public class NotificationHubContext : INotificationHubContext
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationHubContext(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(Guid userId, NotificationDto notification, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group(userId.ToString())
            .SendAsync("ReceiveNotification", notification, cancellationToken);
    }

    public async Task SendUnreadCountAsync(Guid userId, int unreadCount, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group(userId.ToString())
            .SendAsync("UnreadCountUpdated", unreadCount, cancellationToken);
    }
}
