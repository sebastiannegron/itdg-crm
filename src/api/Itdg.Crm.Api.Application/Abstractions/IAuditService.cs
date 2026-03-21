namespace Itdg.Crm.Api.Application.Abstractions;

public interface IAuditService
{
    Task LogAccessAsync(string entityType, Guid entityId, string action, CancellationToken cancellationToken);
}
