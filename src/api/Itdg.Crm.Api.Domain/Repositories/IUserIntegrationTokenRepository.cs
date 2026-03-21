namespace Itdg.Crm.Api.Domain.Repositories;

using Itdg.Crm.Api.Domain.Entities;

public interface IUserIntegrationTokenRepository : IGenericRepository<UserIntegrationToken>
{
    Task<UserIntegrationToken?> GetByUserIdAndProviderAsync(Guid userId, string provider, CancellationToken cancellationToken = default);
}
