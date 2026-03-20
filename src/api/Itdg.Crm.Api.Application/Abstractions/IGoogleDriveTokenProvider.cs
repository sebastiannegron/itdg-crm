namespace Itdg.Crm.Api.Application.Abstractions;

/// <summary>
/// Provides access to the current user's Google OAuth 2.0 access token.
/// </summary>
public interface IGoogleDriveTokenProvider
{
    /// <summary>
    /// Gets the Google OAuth 2.0 access token for the current authenticated user.
    /// </summary>
    /// <returns>The access token, or <c>null</c> if no Google token is available.</returns>
    string? GetAccessToken();
}
