namespace Itdg.Crm.Api.Domain.Repositories;

using Itdg.Crm.Api.Domain.Entities;

public interface IMessageRepository : IGenericRepository<Message>
{
    Task<IReadOnlyList<Message>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<Message?> GetByIdAndClientIdAsync(Guid id, Guid clientId, CancellationToken cancellationToken = default);
}
