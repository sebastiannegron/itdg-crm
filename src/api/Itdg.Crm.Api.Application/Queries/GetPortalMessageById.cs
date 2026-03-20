namespace Itdg.Crm.Api.Application.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;

public record GetPortalMessageById(Guid MessageId, Guid ClientId) : IQuery<MessageDto>;
