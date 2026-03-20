namespace Itdg.Crm.Api.Domain.Repositories;

using Itdg.Crm.Api.Domain.Entities;

public interface IClientAssignmentRepository : IGenericRepository<ClientAssignment>
{
    Task<bool> ExistsAsync(Guid userId, Guid clientId, CancellationToken cancellationToken = default);
    Task<ClientAssignment?> GetByClientAndUserAsync(Guid clientId, Guid userId, CancellationToken cancellationToken = default);
}
