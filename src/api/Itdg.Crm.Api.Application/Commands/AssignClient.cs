namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;

public record AssignClient(Guid ClientId, Guid UserId) : ICommand;
