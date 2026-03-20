namespace Itdg.Crm.Api.Infrastructure.Repositories;

using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Infrastructure.Data;

public class ClientRepository : GenericRepository<Client>, IClientRepository
{
    public ClientRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<Client?> GetByIdWithTierAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Client>()
            .Include(c => c.Tier)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Client> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        ClientStatus? status = null,
        Guid? tierId = null,
        string? search = null,
        Guid? assignedUserId = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Client>()
            .Include(c => c.Tier)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (tierId.HasValue)
        {
            query = query.Where(c => c.TierId == tierId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c =>
                c.Name.Contains(search) ||
                (c.ContactEmail != null && c.ContactEmail.Contains(search)) ||
                (c.IndustryTag != null && c.IndustryTag.Contains(search)));
        }

        if (assignedUserId.HasValue)
        {
            var userId = assignedUserId.Value;
            query = query.Where(c =>
                Context.Set<ClientAssignment>().Any(ca => ca.ClientId == c.Id && ca.UserId == userId));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
