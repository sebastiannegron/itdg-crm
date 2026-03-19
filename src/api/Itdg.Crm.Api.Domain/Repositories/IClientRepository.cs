namespace Itdg.Crm.Api.Domain.Repositories;

using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;

public interface IClientRepository : IGenericRepository<Client>
{
    Task<Client?> GetByIdWithTierAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Client> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        ClientStatus? status = null,
        Guid? tierId = null,
        string? search = null,
        CancellationToken cancellationToken = default);
}
