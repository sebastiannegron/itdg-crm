namespace Itdg.Crm.Api.Infrastructure.Repositories;

using Itdg.Crm.Api.Infrastructure.Data;

public class ClientAssignmentRepository : GenericRepository<ClientAssignment>, IClientAssignmentRepository
{
    public ClientAssignmentRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid clientId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<ClientAssignment>()
            .AnyAsync(ca => ca.UserId == userId && ca.ClientId == clientId, cancellationToken);
    }

    public async Task<ClientAssignment?> GetByClientAndUserAsync(Guid clientId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<ClientAssignment>()
            .FirstOrDefaultAsync(ca => ca.ClientId == clientId && ca.UserId == userId, cancellationToken);
    }
}
