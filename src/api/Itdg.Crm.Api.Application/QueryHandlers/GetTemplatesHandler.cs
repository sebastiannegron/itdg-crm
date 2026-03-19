namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Domain.Repositories;
using Itdg.Crm.Api.Diagnostics;
using Microsoft.Extensions.Logging;

public class GetTemplatesHandler : IQueryHandler<GetTemplates, IEnumerable<CommunicationTemplateDto>>
{
    private readonly ITemplateRepository _repository;
    private readonly ILogger<GetTemplatesHandler> _logger;

    public GetTemplatesHandler(ITemplateRepository repository, ILogger<GetTemplatesHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<CommunicationTemplateDto>> HandleAsync(GetTemplates query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Templates");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Retrieving all templates | CorrelationId: {CorrelationId}", correlationId);

        var templates = await _repository.GetAllAsync(cancellationToken);

        return templates.Select(t => new CommunicationTemplateDto(
            t.Id,
            t.Category,
            t.Name,
            t.SubjectTemplate,
            t.BodyTemplate,
            t.Language,
            t.Version,
            t.IsActive,
            t.CreatedById,
            t.CreatedAt,
            t.UpdatedAt
        ));
    }
}
