namespace Itdg.Crm.Api.Infrastructure.Repositories;

using Itdg.Crm.Api.Infrastructure.Data;

public class UserIntegrationTokenRepository : GenericRepository<UserIntegrationToken>, IUserIntegrationTokenRepository
{
    public UserIntegrationTokenRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<UserIntegrationToken?> GetByUserIdAndProviderAsync(Guid userId, string provider, CancellationToken cancellationToken = default)
    {
        return await Context.Set<UserIntegrationToken>()
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Provider == provider, cancellationToken);
    }
}
