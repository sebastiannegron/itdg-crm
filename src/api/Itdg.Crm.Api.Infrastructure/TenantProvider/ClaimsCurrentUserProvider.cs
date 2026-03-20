namespace Itdg.Crm.Api.Infrastructure.TenantProvider;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Resolves the current user's identity information from JWT claims in the HTTP context.
/// </summary>
public class ClaimsCurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClaimsCurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the Entra (Azure AD) Object ID from the authenticated user's claims.
    /// </summary>
    public string? GetEntraObjectId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
            ?? user?.FindFirst("oid")?.Value;
    }

    /// <summary>
    /// Determines whether the current user is in the specified role.
    /// </summary>
    public bool IsInRole(string role)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.IsInRole(role) ?? false;
    }
}
