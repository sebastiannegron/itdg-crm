namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Domain.GeneralConstants;

public record InviteUser(
    string Email,
    string DisplayName,
    UserRole Role
) : ICommand;
