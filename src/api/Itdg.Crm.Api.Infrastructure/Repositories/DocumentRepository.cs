namespace Itdg.Crm.Api.Infrastructure.Repositories;

using Itdg.Crm.Api.Infrastructure.Data;

public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
{
    public DocumentRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Document>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Document>()
            .Include(d => d.Category)
            .Where(d => d.ClientId == clientId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Document?> GetByIdWithCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Document>()
            .Include(d => d.Category)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }
}
