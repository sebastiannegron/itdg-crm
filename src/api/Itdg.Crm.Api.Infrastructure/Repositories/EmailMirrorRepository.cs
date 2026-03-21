namespace Itdg.Crm.Api.Infrastructure.Repositories;

using Itdg.Crm.Api.Infrastructure.Data;

public class EmailMirrorRepository : GenericRepository<EmailMirror>, IEmailMirrorRepository
{
    public EmailMirrorRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<EmailMirror>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<EmailMirror>()
            .Where(e => e.ClientId == clientId)
            .OrderByDescending(e => e.ReceivedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<EmailMirror> Items, int TotalCount)> GetPagedByClientIdAsync(
        Guid clientId,
        int page,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<EmailMirror>()
            .Where(e => e.ClientId == clientId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e => e.Subject.Contains(search) || e.From.Contains(search) || e.To.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.ReceivedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
