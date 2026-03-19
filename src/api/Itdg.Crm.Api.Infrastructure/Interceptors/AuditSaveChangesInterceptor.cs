namespace Itdg.Crm.Api.Infrastructure.Interceptors;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<AuditSaveChangesInterceptor> _logger;

    public AuditSaveChangesInterceptor(
        IHttpContextAccessor httpContextAccessor,
        ITenantProvider tenantProvider,
        ILogger<AuditSaveChangesInterceptor> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var auditEntries = CreateAuditEntries(eventData.Context);
        if (auditEntries.Count > 0)
        {
            eventData.Context.Set<AuditLog>().AddRange(auditEntries);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private List<AuditLog> CreateAuditEntries(DbContext context)
    {
        var auditEntries = new List<AuditLog>();

        var trackedEntries = context.ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity is not AuditLog)
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (trackedEntries.Count == 0)
        {
            return auditEntries;
        }

        var userId = GetCurrentUserId();
        var tenantId = GetCurrentTenantId();
        var ipAddress = GetCurrentIpAddress();
        var timestamp = DateTimeOffset.UtcNow;

        foreach (var entry in trackedEntries)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = userId,
                EntityType = entry.Metadata.ClrType.Name,
                EntityId = entry.Entity.Id,
                Action = entry.State.ToString(),
                OldValues = GetOldValues(entry),
                NewValues = GetNewValues(entry),
                Timestamp = timestamp,
                IpAddress = ipAddress,
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            };

            auditEntries.Add(auditLog);
        }

        return auditEntries;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }

    private Guid GetCurrentTenantId()
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

    private string? GetCurrentIpAddress()
    {
        return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }

    private static string? GetOldValues(EntityEntry<BaseEntity> entry)
    {
        if (entry.State is EntityState.Added)
        {
            return null;
        }

        var oldValues = new Dictionary<string, object?>();
        foreach (var property in entry.Properties)
        {
            if (entry.State is EntityState.Deleted || property.IsModified)
            {
                oldValues[property.Metadata.Name] = property.OriginalValue;
            }
        }

        return oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null;
    }

    private static string? GetNewValues(EntityEntry<BaseEntity> entry)
    {
        if (entry.State is EntityState.Deleted)
        {
            return null;
        }

        var newValues = new Dictionary<string, object?>();
        foreach (var property in entry.Properties)
        {
            if (entry.State is EntityState.Added || property.IsModified)
            {
                newValues[property.Metadata.Name] = property.CurrentValue;
            }
        }

        return newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null;
    }
}
