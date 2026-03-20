namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetPortalMessageByIdHandler : IQueryHandler<GetPortalMessageById, MessageDto>
{
    private readonly IMessageRepository _repository;
    private readonly ILogger<GetPortalMessageByIdHandler> _logger;

    public GetPortalMessageByIdHandler(IMessageRepository repository, ILogger<GetPortalMessageByIdHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<MessageDto> HandleAsync(GetPortalMessageById query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Portal Message By Id");
        activity?.SetTag("CorrelationId", correlationId);
        activity?.SetTag("MessageId", query.MessageId);
        activity?.SetTag("ClientId", query.ClientId);

        _logger.LogInformation("Getting portal message {MessageId} for client {ClientId} | CorrelationId: {CorrelationId}", query.MessageId, query.ClientId, correlationId);

        var message = await _repository.GetByIdAndClientIdAsync(query.MessageId, query.ClientId, cancellationToken);

        if (message is null)
        {
            throw new NotFoundException("Message", query.MessageId);
        }

        return new MessageDto(
            message.Id,
            message.ClientId,
            message.SenderId,
            message.Direction.ToString(),
            message.Subject,
            message.Body,
            message.TemplateId,
            message.IsPortalMessage,
            message.IsRead,
            message.Attachments,
            message.CreatedAt);
    }
}
