namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Domain.GeneralConstants;

public record UpdateUser(
    Guid UserId,
    UserRole Role,
    bool IsActive
) : ICommand;
