namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;

public record UnassignClient(Guid ClientId, Guid UserId) : ICommand;
