namespace Itdg.Crm.Api.Domain.Repositories;

using Itdg.Crm.Api.Domain.Entities;

public interface IEmailMirrorRepository : IGenericRepository<EmailMirror>
{
    Task<IReadOnlyList<EmailMirror>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
}
