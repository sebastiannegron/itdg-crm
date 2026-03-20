namespace Itdg.Crm.Api.Application.Abstractions;

/// <summary>
/// Provides access to the current authenticated user's identity information from claims.
/// </summary>
public interface ICurrentUserProvider
{
    /// <summary>
    /// Gets the Entra (Azure AD) Object ID from the authenticated user's claims.
    /// </summary>
    /// <returns>The Entra Object ID, or <c>null</c> if not present.</returns>
    string? GetEntraObjectId();

    /// <summary>
    /// Determines whether the current user is in the specified role.
    /// </summary>
    /// <param name="role">The role name to check.</param>
    /// <returns><c>true</c> if the user is in the role; otherwise, <c>false</c>.</returns>
    bool IsInRole(string role);
}
