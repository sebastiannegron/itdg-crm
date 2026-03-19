namespace Itdg.Crm.Api.Infrastructure.Repositories;

using Itdg.Crm.Api.Infrastructure.Data;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEntraObjectIdAsync(string entraObjectId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<User>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId, cancellationToken);
    }
}
