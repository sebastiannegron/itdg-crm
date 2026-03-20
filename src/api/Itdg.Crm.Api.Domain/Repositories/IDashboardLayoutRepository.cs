namespace Itdg.Crm.Api.Domain.Repositories;

using Itdg.Crm.Api.Domain.Entities;

public interface IDashboardLayoutRepository : IGenericRepository<DashboardLayout>
{
    Task<DashboardLayout?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
