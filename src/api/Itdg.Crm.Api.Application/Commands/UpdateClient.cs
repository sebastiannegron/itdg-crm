namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Domain.GeneralConstants;

public record UpdateClient(
    Guid ClientId,
    string Name,
    string? ContactEmail,
    string? Phone,
    string? Address,
    Guid? TierId,
    ClientStatus Status,
    string? IndustryTag,
    string? Notes,
    string? CustomFields
) : ICommand;
