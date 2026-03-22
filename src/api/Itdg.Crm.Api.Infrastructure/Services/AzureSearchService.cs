namespace Itdg.Crm.Api.Infrastructure.Services;

using System.Diagnostics;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class AzureSearchService : ISearchService
{
    private readonly AzureSearchOptions _options;
    private readonly ILogger<AzureSearchService> _logger;

    public AzureSearchService(IOptions<AzureSearchOptions> options, ILogger<AzureSearchService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task IndexDocumentAsync(SearchDocumentDto document, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Index Document in Search");
        activity?.SetTag("DocumentId", document.DocumentId);

        _logger.LogInformation("Indexing document {DocumentId} in Azure AI Search", document.DocumentId);

        SearchClient client = CreateSearchClient();

        var searchDocument = MapToIndexDocument(document);

        await client.MergeOrUploadDocumentsAsync(new[] { searchDocument }, new IndexDocumentsOptions(), cancellationToken);

        _logger.LogInformation("Successfully indexed document {DocumentId} in Azure AI Search", document.DocumentId);
    }

    public async Task RemoveDocumentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Remove Document from Search");
        activity?.SetTag("DocumentId", documentId);

        _logger.LogInformation("Removing document {DocumentId} from Azure AI Search index", documentId);

        SearchClient client = CreateSearchClient();

        await client.DeleteDocumentsAsync("documentId", new[] { documentId.ToString() }, new IndexDocumentsOptions(), cancellationToken);

        _logger.LogInformation("Successfully removed document {DocumentId} from Azure AI Search index", documentId);
    }

    public async Task<IReadOnlyList<SearchDocumentDto>> SearchDocumentsAsync(string query, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Search Documents");
        activity?.SetTag("Query", query);

        _logger.LogInformation("Searching documents with query: {Query}", query);

        SearchClient client = CreateSearchClient();

        SearchOptions searchOptions = new()
        {
            IncludeTotalCount = true,
            Select = { "documentId", "clientId", "clientName", "fileName", "category", "content", "uploadedAt" }
        };

        SearchResults<SearchIndexDocument> results = await client.SearchAsync<SearchIndexDocument>(query, searchOptions, cancellationToken);

        List<SearchDocumentDto> documents = [];

        await foreach (SearchResult<SearchIndexDocument> result in results.GetResultsAsync())
        {
            documents.Add(MapFromIndexDocument(result.Document));
        }

        _logger.LogInformation("Search returned {Count} documents for query: {Query}", documents.Count, query);

        return documents;
    }

    public async Task<(IReadOnlyList<DocumentSearchResultDto> Items, int TotalCount)> SearchDocumentsAsync(
        string query,
        Guid? clientId,
        string? category,
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Search Documents With Filters");
        activity?.SetTag("Query", query);

        _logger.LogInformation("Searching documents with query: {Query}, filters: clientId={ClientId}, category={Category}", query, clientId, category);

        SearchClient client = CreateSearchClient();

        SearchOptions searchOptions = new()
        {
            IncludeTotalCount = true,
            Skip = (page - 1) * pageSize,
            Size = pageSize,
            Select = { "documentId", "clientId", "clientName", "fileName", "category", "content", "uploadedAt" },
            HighlightFields = { "content" },
            HighlightPreTag = "<em>",
            HighlightPostTag = "</em>"
        };

        string? filter = BuildFilter(clientId, category, dateFrom, dateTo);
        if (filter is not null)
        {
            searchOptions.Filter = filter;
        }

        SearchResults<SearchIndexDocument> results = await client.SearchAsync<SearchIndexDocument>(query, searchOptions, cancellationToken);

        List<DocumentSearchResultDto> documents = [];

        await foreach (SearchResult<SearchIndexDocument> result in results.GetResultsAsync())
        {
            string? snippet = null;
            if (result.Highlights is not null && result.Highlights.TryGetValue("content", out var highlights) && highlights.Count > 0)
            {
                snippet = string.Join(" … ", highlights);
            }
            else if (!string.IsNullOrEmpty(result.Document.Content))
            {
                snippet = result.Document.Content.Length > 200
                    ? result.Document.Content[..200] + "…"
                    : result.Document.Content;
            }

            documents.Add(new DocumentSearchResultDto(
                DocumentId: Guid.Parse(result.Document.DocumentId),
                ClientId: Guid.Parse(result.Document.ClientId),
                ClientName: result.Document.ClientName,
                FileName: result.Document.FileName,
                Category: result.Document.Category,
                UploadedAt: result.Document.UploadedAt,
                RelevanceSnippet: snippet
            ));
        }

        int totalCount = (int)(results.TotalCount ?? documents.Count);

        _logger.LogInformation("Search returned {Count} of {Total} documents for query: {Query}", documents.Count, totalCount, query);

        return (documents, totalCount);
    }

    internal static string? BuildFilter(Guid? clientId, string? category, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
    {
        List<string> filters = [];

        if (clientId.HasValue)
        {
            filters.Add($"clientId eq '{clientId.Value}'");
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            filters.Add($"category eq '{category.Replace("'", "''")}'");
        }

        if (dateFrom.HasValue)
        {
            filters.Add($"uploadedAt ge {dateFrom.Value:O}");
        }

        if (dateTo.HasValue)
        {
            filters.Add($"uploadedAt le {dateTo.Value:O}");
        }

        return filters.Count > 0 ? string.Join(" and ", filters) : null;
    }

    internal SearchClient CreateSearchClient()
    {
        return new SearchClient(
            new Uri(_options.Endpoint),
            _options.IndexName,
            new AzureKeyCredential(_options.ApiKey));
    }

    internal static SearchIndex CreateIndexDefinition(string indexName)
    {
        return new SearchIndex(indexName)
        {
            Fields =
            [
                new SimpleField("documentId", SearchFieldDataType.String) { IsKey = true, IsFilterable = true },
                new SimpleField("clientId", SearchFieldDataType.String) { IsFilterable = true },
                new SearchableField("clientName") { IsFilterable = true, IsSortable = true },
                new SearchableField("fileName") { IsFilterable = true, IsSortable = true },
                new SearchableField("category") { IsFilterable = true, IsSortable = true },
                new SearchableField("content"),
                new SimpleField("uploadedAt", SearchFieldDataType.DateTimeOffset) { IsFilterable = true, IsSortable = true }
            ]
        };
    }

    internal static SearchIndexDocument MapToIndexDocument(SearchDocumentDto dto)
    {
        return new SearchIndexDocument
        {
            DocumentId = dto.DocumentId.ToString(),
            ClientId = dto.ClientId.ToString(),
            ClientName = dto.ClientName,
            FileName = dto.FileName,
            Category = dto.Category,
            Content = dto.Content,
            UploadedAt = dto.UploadedAt
        };
    }

    internal static SearchDocumentDto MapFromIndexDocument(SearchIndexDocument doc)
    {
        return new SearchDocumentDto(
            Guid.Parse(doc.DocumentId),
            Guid.Parse(doc.ClientId),
            doc.ClientName,
            doc.FileName,
            doc.Category,
            doc.Content,
            doc.UploadedAt
        );
    }
}

internal class SearchIndexDocument
{
    public required string DocumentId { get; set; }
    public required string ClientId { get; set; }
    public required string ClientName { get; set; }
    public required string FileName { get; set; }
    public required string Category { get; set; }
    public string? Content { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
}
