namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;

public record MarkNotificationAsRead(Guid NotificationId) : ICommand;
