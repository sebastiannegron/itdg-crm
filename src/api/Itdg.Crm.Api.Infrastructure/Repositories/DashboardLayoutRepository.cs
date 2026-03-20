namespace Itdg.Crm.Api.Infrastructure.Repositories;

using Itdg.Crm.Api.Infrastructure.Data;

public class DashboardLayoutRepository : GenericRepository<DashboardLayout>, IDashboardLayoutRepository
{
    public DashboardLayoutRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<DashboardLayout?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<DashboardLayout>()
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
    }
}
