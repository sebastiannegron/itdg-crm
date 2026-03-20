namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;

public record UpdateNotificationPreferences(IReadOnlyList<NotificationPreferenceDto> Preferences) : ICommand;
