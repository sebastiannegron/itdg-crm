namespace Itdg.Crm.Api.Application.Abstractions;

/// <summary>
/// Provides access to the current tenant context from authenticated user claims.
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// Gets the current tenant ID from the authenticated user's claims.
    /// </summary>
    /// <returns>The tenant ID for the current request context.</returns>
    Guid GetTenantId();
}
