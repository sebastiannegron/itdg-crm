namespace Itdg.Crm.Api.Domain.Repositories;

using Itdg.Crm.Api.Domain.Entities;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEntraObjectIdAsync(string entraObjectId, CancellationToken cancellationToken = default);
}
