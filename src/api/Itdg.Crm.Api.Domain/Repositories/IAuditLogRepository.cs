namespace Itdg.Crm.Api.Domain.Repositories;

using Itdg.Crm.Api.Domain.Entities;

public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetByEntityAsync(
        string entityType,
        Guid entityId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
