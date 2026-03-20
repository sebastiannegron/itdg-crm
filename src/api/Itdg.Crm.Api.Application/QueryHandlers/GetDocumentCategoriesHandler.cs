namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetDocumentCategoriesHandler : IQueryHandler<GetDocumentCategories, IEnumerable<DocumentCategoryDto>>
{
    private readonly IGenericRepository<DocumentCategory> _repository;
    private readonly ILogger<GetDocumentCategoriesHandler> _logger;

    public GetDocumentCategoriesHandler(
        IGenericRepository<DocumentCategory> repository,
        ILogger<GetDocumentCategoriesHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<DocumentCategoryDto>> HandleAsync(GetDocumentCategories query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Document Categories");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Getting all document categories | CorrelationId: {CorrelationId}", correlationId);

        var categories = await _repository.GetAllAsync(cancellationToken);

        return categories
            .OrderBy(c => c.SortOrder)
            .Select(c => new DocumentCategoryDto(
                c.Id,
                c.Name,
                c.NamingConvention,
                c.IsDefault,
                c.SortOrder,
                c.CreatedAt,
                c.UpdatedAt
            ));
    }
}
