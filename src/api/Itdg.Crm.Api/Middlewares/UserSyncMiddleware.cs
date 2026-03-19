namespace Itdg.Crm.Api.Middlewares;

using System.Security.Claims;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;

public class UserSyncMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserSyncMiddleware> _logger;

    public UserSyncMiddleware(RequestDelegate next, ILogger<UserSyncMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            await SyncUserAsync(context.User, userRepository, context.RequestAborted);
        }

        await _next(context);
    }

    private async Task SyncUserAsync(ClaimsPrincipal principal, IUserRepository userRepository, CancellationToken cancellationToken)
    {
        var entraObjectId = principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
            ?? principal.FindFirst("oid")?.Value;

        if (string.IsNullOrWhiteSpace(entraObjectId))
        {
            return;
        }

        var existingUser = await userRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);
        if (existingUser is not null)
        {
            return;
        }

        var tenantIdClaim = principal.FindFirst("TenantId")?.Value;
        if (string.IsNullOrWhiteSpace(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            _logger.LogWarning("Cannot sync user: TenantId claim is missing or invalid for EntraObjectId {EntraObjectId}", entraObjectId);
            return;
        }

        var email = principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("preferred_username")?.Value
            ?? string.Empty;

        var displayName = principal.FindFirst("name")?.Value
            ?? principal.FindFirst(ClaimTypes.Name)?.Value
            ?? email;

        var roleString = principal.FindFirst(ClaimTypes.Role)?.Value
            ?? principal.FindFirst("roles")?.Value;

        var role = Enum.TryParse<UserRole>(roleString, ignoreCase: true, out var parsedRole)
            ? parsedRole
            : UserRole.Associate;

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EntraObjectId = entraObjectId,
            Email = email,
            DisplayName = displayName,
            Role = role,
            IsActive = true
        };

        try
        {
            await userRepository.AddAsync(user, cancellationToken);
            _logger.LogInformation("Auto-created user record for EntraObjectId {EntraObjectId} in tenant {TenantId}", entraObjectId, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to auto-create user record for EntraObjectId {EntraObjectId}. User may have been created by a concurrent request", entraObjectId);
        }
    }
}
