namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;

public record SendPortalMessage(Guid ClientId, Guid SenderId, string Subject, string Body) : ICommand;
