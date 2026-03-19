namespace Itdg.Crm.Api.Infrastructure.TenantProvider;

using Microsoft.AspNetCore.Http;
using System.Security.Claims;

/// <summary>
/// Resolves the current tenant ID from JWT claims in the HTTP context.
/// </summary>
public class ClaimsTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClaimsTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the current tenant ID from the authenticated user's claims.
    /// </summary>
    /// <returns>The tenant ID for the current request context.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no HTTP context is available or user is not authenticated.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the TenantId claim is missing or invalid.</exception>
    public Guid GetTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new InvalidOperationException("No HTTP context available.");
        }

        var user = httpContext.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new InvalidOperationException("User is not authenticated.");
        }

        var tenantIdClaim = user.FindFirst("TenantId")?.Value;
        if (string.IsNullOrWhiteSpace(tenantIdClaim))
        {
            throw new UnauthorizedAccessException("TenantId claim is missing from the user's token.");
        }

        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            throw new UnauthorizedAccessException("TenantId claim is not a valid GUID.");
        }

        return tenantId;
    }
}
