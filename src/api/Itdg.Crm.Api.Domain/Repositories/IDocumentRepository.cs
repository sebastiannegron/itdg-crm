namespace Itdg.Crm.Api.Domain.Repositories;

using Itdg.Crm.Api.Domain.Entities;

public interface IDocumentRepository : IGenericRepository<Document>
{
    Task<IReadOnlyList<Document>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<Document?> GetByIdWithCategoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Document> Items, int TotalCount)> GetPagedByClientIdAsync(
        Guid clientId,
        int page,
        int pageSize,
        Guid? categoryId = null,
        int? year = null,
        string? search = null,
        CancellationToken cancellationToken = default);
    Task<Document?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Document> Items, int TotalCount)> GetDeletedDocumentsPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> GetDocumentsDeletedBeforeAsync(
        DateTimeOffset cutoffDate,
        CancellationToken cancellationToken = default);
    Task<(Document? Document, IReadOnlyList<DocumentVersion> Versions)> GetByIdWithVersionsAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
