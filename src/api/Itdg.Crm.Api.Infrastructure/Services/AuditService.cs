namespace Itdg.Crm.Api.Infrastructure.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

public class AuditService : IAuditService
{
    private readonly DbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        DbContext context,
        ITenantProvider tenantProvider,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditService> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAccessAsync(string entityType, Guid entityId, string action, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = GetTenantId(),
            UserId = GetCurrentUserId(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Timestamp = now,
            IpAddress = GetIpAddress(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.Set<AuditLog>().AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit log created: {Action} on {EntityType} {EntityId}",
            action, entityType, entityId);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private Guid GetTenantId()
    {
        try
        {
            return _tenantProvider.GetTenantId();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not resolve tenant ID for audit log");
            return Guid.Empty;
        }
    }

    private string? GetIpAddress()
    {
        return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }
}
