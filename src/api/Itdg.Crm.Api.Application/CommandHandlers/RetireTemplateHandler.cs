namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Repositories;
using Itdg.Crm.Api.Diagnostics;
using Microsoft.Extensions.Logging;

public class RetireTemplateHandler : ICommandHandler<RetireTemplate>
{
    private readonly ITemplateRepository _repository;
    private readonly ILogger<RetireTemplateHandler> _logger;

    public RetireTemplateHandler(ITemplateRepository repository, ILogger<RetireTemplateHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(RetireTemplate command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Retire Template");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Retiring template {TemplateId} | CorrelationId: {CorrelationId}", command.Id, correlationId);

        var template = await _repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("Template", command.Id);

        template.IsActive = false;
        template.DeletedAt = DateTimeOffset.UtcNow;

        await _repository.UpdateAsync(template, cancellationToken);

        _logger.LogInformation("Template {TemplateId} retired | CorrelationId: {CorrelationId}", command.Id, correlationId);
    }
}
