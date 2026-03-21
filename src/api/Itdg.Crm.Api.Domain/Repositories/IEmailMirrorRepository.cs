namespace Itdg.Crm.Api.Domain.Repositories;

using Itdg.Crm.Api.Domain.Entities;

public interface IEmailMirrorRepository : IGenericRepository<EmailMirror>
{
    Task<IReadOnlyList<EmailMirror>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<EmailMirror> Items, int TotalCount)> GetPagedByClientIdAsync(
        Guid clientId,
        int page,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default);
}
