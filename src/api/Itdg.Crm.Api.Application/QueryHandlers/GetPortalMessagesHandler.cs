namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetPortalMessagesHandler : IQueryHandler<GetPortalMessages, IEnumerable<MessageDto>>
{
    private readonly IMessageRepository _repository;
    private readonly ILogger<GetPortalMessagesHandler> _logger;

    public GetPortalMessagesHandler(IMessageRepository repository, ILogger<GetPortalMessagesHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<MessageDto>> HandleAsync(GetPortalMessages query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Portal Messages");
        activity?.SetTag("CorrelationId", correlationId);
        activity?.SetTag("ClientId", query.ClientId);

        _logger.LogInformation("Getting portal messages for client {ClientId} | CorrelationId: {CorrelationId}", query.ClientId, correlationId);

        var messages = await _repository.GetByClientIdAsync(query.ClientId, cancellationToken);

        return messages.Select(m => new MessageDto(
            m.Id,
            m.ClientId,
            m.SenderId,
            m.Direction.ToString(),
            m.Subject,
            m.Body,
            m.TemplateId,
            m.IsPortalMessage,
            m.IsRead,
            m.Attachments,
            m.CreatedAt));
    }
}
