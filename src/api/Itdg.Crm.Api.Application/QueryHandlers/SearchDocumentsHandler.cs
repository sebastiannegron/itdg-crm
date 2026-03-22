namespace Itdg.Crm.Api.Application.QueryHandlers;

using System.Diagnostics;
using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Microsoft.Extensions.Logging;

public class SearchDocumentsHandler : IQueryHandler<SearchDocuments, PaginatedResultDto<DocumentSearchResultDto>>
{
    private readonly ISearchService _searchService;
    private readonly ILogger<SearchDocumentsHandler> _logger;

    public SearchDocumentsHandler(ISearchService searchService, ILogger<SearchDocumentsHandler> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    public async Task<PaginatedResultDto<DocumentSearchResultDto>> HandleAsync(
        SearchDocuments query,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Search Documents");
        activity?.SetTag("CorrelationId", correlationId);
        activity?.SetTag("Query", query.Query);

        _logger.LogInformation(
            "Searching documents with query '{Query}' page {Page} | CorrelationId: {CorrelationId}",
            query.Query, query.Page, correlationId);

        var (items, totalCount) = await _searchService.SearchDocumentsAsync(
            query.Query,
            query.ClientId,
            query.Category,
            query.DateFrom,
            query.DateTo,
            query.Page,
            query.PageSize,
            cancellationToken);

        return new PaginatedResultDto<DocumentSearchResultDto>(
            Items: items,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize
        );
    }
}
