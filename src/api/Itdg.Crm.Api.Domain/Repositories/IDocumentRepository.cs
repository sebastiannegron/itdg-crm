namespace Itdg.Crm.Api.Domain.Repositories;

using Itdg.Crm.Api.Domain.Entities;

public interface IDocumentRepository : IGenericRepository<Document>
{
    Task<IReadOnlyList<Document>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<Document?> GetByIdWithCategoryAsync(Guid id, CancellationToken cancellationToken = default);
}
