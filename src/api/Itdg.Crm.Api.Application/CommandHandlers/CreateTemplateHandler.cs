namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Itdg.Crm.Api.Diagnostics;
using Microsoft.Extensions.Logging;

public class CreateTemplateHandler : ICommandHandler<CreateTemplate>
{
    private readonly ITemplateRepository _repository;
    private readonly ILogger<CreateTemplateHandler> _logger;

    public CreateTemplateHandler(ITemplateRepository repository, ILogger<CreateTemplateHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(CreateTemplate command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Create Template");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Creating template '{Name}' | CorrelationId: {CorrelationId}", command.Name, correlationId);

        var template = new CommunicationTemplate
        {
            Id = Guid.NewGuid(),
            Category = command.Category,
            Name = command.Name,
            SubjectTemplate = command.SubjectTemplate,
            BodyTemplate = command.BodyTemplate,
            Language = command.Language,
            Version = 1,
            IsActive = true,
            CreatedById = command.CreatedById
        };

        await _repository.AddAsync(template, cancellationToken);

        _logger.LogInformation("Template '{Name}' created with Id {TemplateId} | CorrelationId: {CorrelationId}", command.Name, template.Id, correlationId);
    }
}
