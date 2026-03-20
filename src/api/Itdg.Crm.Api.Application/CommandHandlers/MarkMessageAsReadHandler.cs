namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class MarkMessageAsReadHandler : ICommandHandler<MarkMessageAsRead>
{
    private readonly IMessageRepository _repository;
    private readonly ILogger<MarkMessageAsReadHandler> _logger;

    public MarkMessageAsReadHandler(IMessageRepository repository, ILogger<MarkMessageAsReadHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(MarkMessageAsRead command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Mark Message As Read");
        activity?.SetTag("CorrelationId", correlationId);
        activity?.SetTag("MessageId", command.MessageId);
        activity?.SetTag("ClientId", command.ClientId);

        _logger.LogInformation("Marking message {MessageId} as read for client {ClientId} | CorrelationId: {CorrelationId}", command.MessageId, command.ClientId, correlationId);

        var message = await _repository.GetByIdAndClientIdAsync(command.MessageId, command.ClientId, cancellationToken);

        if (message is null)
        {
            throw new NotFoundException("Message", command.MessageId);
        }

        message.IsRead = true;
        await _repository.UpdateAsync(message, cancellationToken);

        _logger.LogInformation("Message {MessageId} marked as read for client {ClientId} | CorrelationId: {CorrelationId}", command.MessageId, command.ClientId, correlationId);
    }
}
