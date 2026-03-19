namespace Itdg.Crm.Api.Infrastructure.Repositories;

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
}
