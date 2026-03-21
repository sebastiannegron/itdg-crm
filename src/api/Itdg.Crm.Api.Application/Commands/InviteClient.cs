namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;

public record InviteClient(
    Guid ClientId,
    string Email
) : ICommand;
