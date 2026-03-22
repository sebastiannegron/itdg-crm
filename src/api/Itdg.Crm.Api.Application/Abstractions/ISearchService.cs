namespace Itdg.Crm.Api.Application.Abstractions;

using Itdg.Crm.Api.Application.Dtos;

public interface ISearchService
{
    Task IndexDocumentAsync(SearchDocumentDto document, CancellationToken cancellationToken);

    Task RemoveDocumentAsync(Guid documentId, CancellationToken cancellationToken);

    Task<IReadOnlyList<SearchDocumentDto>> SearchDocumentsAsync(string query, CancellationToken cancellationToken);

    Task<(IReadOnlyList<DocumentSearchResultDto> Items, int TotalCount)> SearchDocumentsAsync(
        string query,
        Guid? clientId,
        string? category,
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
