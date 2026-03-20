namespace Itdg.Crm.Api.Infrastructure.Repositories;

using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Infrastructure.Data;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(CrmDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Looks up a user by their Entra ID object identifier across all tenants.
    /// Uses IgnoreQueryFilters() because EntraObjectId is globally unique and this
    /// method is called during authentication before tenant context is established.
    /// </summary>
    public async Task<User?> GetByEntraObjectIdAsync(string entraObjectId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<User>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await Context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        UserRole? role = null,
        bool? isActive = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<User>().AsQueryable();

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u =>
                u.DisplayName.Contains(search) ||
                u.Email.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(u => u.DisplayName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
