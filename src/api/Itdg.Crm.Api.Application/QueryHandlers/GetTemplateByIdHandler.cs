namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Domain.Repositories;
using Itdg.Crm.Api.Diagnostics;
using Microsoft.Extensions.Logging;

public class GetTemplateByIdHandler : IQueryHandler<GetTemplateById, CommunicationTemplateDto>
{
    private readonly ITemplateRepository _repository;
    private readonly ILogger<GetTemplateByIdHandler> _logger;

    public GetTemplateByIdHandler(ITemplateRepository repository, ILogger<GetTemplateByIdHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CommunicationTemplateDto> HandleAsync(GetTemplateById query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Template By Id");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Retrieving template {TemplateId} | CorrelationId: {CorrelationId}", query.Id, correlationId);

        var template = await _repository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new NotFoundException("Template", query.Id);

        return new CommunicationTemplateDto(
            template.Id,
            template.Category,
            template.Name,
            template.SubjectTemplate,
            template.BodyTemplate,
            template.Language,
            template.Version,
            template.IsActive,
            template.CreatedById,
            template.CreatedAt,
            template.UpdatedAt
        );
    }
}
