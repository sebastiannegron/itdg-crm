namespace Itdg.Crm.Api.Infrastructure.TenantProvider;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Resolves the current user's Google OAuth 2.0 access token from JWT claims in the HTTP context.
/// </summary>
public class ClaimsGoogleDriveTokenProvider : IGoogleDriveTokenProvider
{
    private const string GoogleAccessTokenClaim = "GoogleAccessToken";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClaimsGoogleDriveTokenProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the Google OAuth 2.0 access token from the authenticated user's claims.
    /// </summary>
    /// <returns>The access token, or <c>null</c> if not present.</returns>
    public string? GetAccessToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        var user = httpContext.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return user.FindFirst(GoogleAccessTokenClaim)?.Value;
    }
}
