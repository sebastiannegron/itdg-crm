namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Domain.Repositories;
using Itdg.Crm.Api.Diagnostics;
using Microsoft.Extensions.Logging;

public class RenderTemplateHandler : IQueryHandler<RenderTemplate, RenderedTemplateDto>
{
    private readonly ITemplateRepository _repository;
    private readonly ITemplateRenderer _renderer;
    private readonly ILogger<RenderTemplateHandler> _logger;

    public RenderTemplateHandler(
        ITemplateRepository repository,
        ITemplateRenderer renderer,
        ILogger<RenderTemplateHandler> logger)
    {
        _repository = repository;
        _renderer = renderer;
        _logger = logger;
    }

    public async Task<RenderedTemplateDto> HandleAsync(RenderTemplate query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Render Template");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Rendering template {TemplateId} | CorrelationId: {CorrelationId}", query.TemplateId, correlationId);

        var template = await _repository.GetByIdAsync(query.TemplateId, cancellationToken)
            ?? throw new NotFoundException("Template", query.TemplateId);

        var renderedSubject = _renderer.Render(template.SubjectTemplate, query.MergeFields);
        var renderedBody = _renderer.Render(template.BodyTemplate, query.MergeFields);

        _logger.LogInformation("Successfully rendered template {TemplateId} | CorrelationId: {CorrelationId}", query.TemplateId, correlationId);

        return new RenderedTemplateDto(renderedSubject, renderedBody);
    }
}
