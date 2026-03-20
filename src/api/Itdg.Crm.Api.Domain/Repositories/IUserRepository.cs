namespace Itdg.Crm.Api.Domain.Repositories;

using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEntraObjectIdAsync(string entraObjectId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        UserRole? role = null,
        bool? isActive = null,
        string? search = null,
        CancellationToken cancellationToken = default);
}
