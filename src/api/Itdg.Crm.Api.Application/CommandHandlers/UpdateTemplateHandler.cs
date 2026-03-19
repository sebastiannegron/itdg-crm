namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Repositories;
using Itdg.Crm.Api.Diagnostics;
using Microsoft.Extensions.Logging;

public class UpdateTemplateHandler : ICommandHandler<UpdateTemplate>
{
    private readonly ITemplateRepository _repository;
    private readonly ILogger<UpdateTemplateHandler> _logger;

    public UpdateTemplateHandler(ITemplateRepository repository, ILogger<UpdateTemplateHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateTemplate command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Update Template");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Updating template {TemplateId} | CorrelationId: {CorrelationId}", command.Id, correlationId);

        var template = await _repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("Template", command.Id);

        template.Category = command.Category;
        template.Name = command.Name;
        template.SubjectTemplate = command.SubjectTemplate;
        template.BodyTemplate = command.BodyTemplate;
        template.Language = command.Language;
        template.Version += 1;

        await _repository.UpdateAsync(template, cancellationToken);

        _logger.LogInformation("Template {TemplateId} updated to version {Version} | CorrelationId: {CorrelationId}", command.Id, template.Version, correlationId);
    }
}
