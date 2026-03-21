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

    public async Task<(IReadOnlyList<Document> Items, int TotalCount)> GetPagedByClientIdAsync(
        Guid clientId,
        int page,
        int pageSize,
        Guid? categoryId = null,
        int? year = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Document>()
            .Include(d => d.Category)
            .Where(d => d.ClientId == clientId)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(d => d.CategoryId == categoryId.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(d => d.CreatedAt.Year == year.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d => d.FileName.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Document?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Document>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Document> Items, int TotalCount)> GetDeletedDocumentsPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Document>()
            .IgnoreQueryFilters()
            .Include(d => d.Category)
            .Where(d => d.DeletedAt != null);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(d => d.DeletedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Document>> GetDocumentsDeletedBeforeAsync(
        DateTimeOffset cutoffDate,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Document>()
            .IgnoreQueryFilters()
            .Where(d => d.DeletedAt != null && d.DeletedAt < cutoffDate)
            .ToListAsync(cancellationToken);
    }
}
