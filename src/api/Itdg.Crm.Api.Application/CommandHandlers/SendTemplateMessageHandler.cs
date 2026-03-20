namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class SendTemplateMessageHandler : ICommandHandler<SendTemplateMessage>
{
    private readonly ITemplateRepository _templateRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ITemplateRenderer _renderer;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<SendTemplateMessageHandler> _logger;

    public SendTemplateMessageHandler(
        ITemplateRepository templateRepository,
        IMessageRepository messageRepository,
        ITemplateRenderer renderer,
        IEmailSender emailSender,
        ILogger<SendTemplateMessageHandler> logger)
    {
        _templateRepository = templateRepository;
        _messageRepository = messageRepository;
        _renderer = renderer;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task HandleAsync(SendTemplateMessage command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Send Template Message");
        activity?.SetTag("CorrelationId", correlationId);
        activity?.SetTag("TemplateId", command.TemplateId);
        activity?.SetTag("ClientId", command.ClientId);

        _logger.LogInformation(
            "Sending template message for template {TemplateId} to client {ClientId} | CorrelationId: {CorrelationId}",
            command.TemplateId, command.ClientId, correlationId);

        var template = await _templateRepository.GetByIdAsync(command.TemplateId, cancellationToken)
            ?? throw new NotFoundException("Template", command.TemplateId);

        var renderedSubject = _renderer.Render(template.SubjectTemplate, command.MergeFields);
        var renderedBody = _renderer.Render(template.BodyTemplate, command.MergeFields);

        if (command.SendViaPortal)
        {
            var message = new Message
            {
                Id = Guid.NewGuid(),
                ClientId = command.ClientId,
                SenderId = command.SenderId,
                Direction = MessageDirection.Outbound,
                Subject = renderedSubject,
                Body = renderedBody,
                TemplateId = command.TemplateId,
                IsPortalMessage = true,
                IsRead = false
            };

            await _messageRepository.AddAsync(message, cancellationToken);

            _logger.LogInformation(
                "Portal message {MessageId} created for client {ClientId} | CorrelationId: {CorrelationId}",
                message.Id, command.ClientId, correlationId);
        }

        if (command.SendViaEmail && !string.IsNullOrWhiteSpace(command.RecipientEmail))
        {
            await _emailSender.SendAsync(command.RecipientEmail, renderedSubject, renderedBody, cancellationToken);

            _logger.LogInformation(
                "Email sent to {RecipientEmail} for client {ClientId} | CorrelationId: {CorrelationId}",
                command.RecipientEmail, command.ClientId, correlationId);
        }

        _logger.LogInformation(
            "Template message sent successfully for template {TemplateId} to client {ClientId} | CorrelationId: {CorrelationId}",
            command.TemplateId, command.ClientId, correlationId);
    }
}
