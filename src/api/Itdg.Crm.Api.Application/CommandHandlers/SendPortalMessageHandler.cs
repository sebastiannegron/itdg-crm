namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class SendPortalMessageHandler : ICommandHandler<SendPortalMessage>
{
    private readonly IMessageRepository _repository;
    private readonly ILogger<SendPortalMessageHandler> _logger;

    public SendPortalMessageHandler(IMessageRepository repository, ILogger<SendPortalMessageHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(SendPortalMessage command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Send Portal Message");
        activity?.SetTag("CorrelationId", correlationId);
        activity?.SetTag("ClientId", command.ClientId);

        _logger.LogInformation("Sending portal message for client {ClientId} | CorrelationId: {CorrelationId}", command.ClientId, correlationId);

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ClientId = command.ClientId,
            SenderId = command.SenderId,
            Direction = MessageDirection.Inbound,
            Subject = command.Subject,
            Body = command.Body,
            IsPortalMessage = true,
            IsRead = false
        };

        await _repository.AddAsync(message, cancellationToken);

        _logger.LogInformation("Portal message {MessageId} sent for client {ClientId} | CorrelationId: {CorrelationId}", message.Id, command.ClientId, correlationId);
    }
}
