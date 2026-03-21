namespace Itdg.Crm.Api.Application.Abstractions;

public interface IPortalConfiguration
{
    /// <summary>
    /// Gets the base URL for the client portal (e.g., "https://portal.itdg.com").
    /// </summary>
    string GetBaseUrl();

    /// <summary>
    /// Gets the number of days before an invitation expires.
    /// </summary>
    int GetInvitationExpiryDays();
}
