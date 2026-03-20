namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;

public record MarkMessageAsRead(Guid MessageId, Guid ClientId) : ICommand;
