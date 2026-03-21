namespace Itdg.Crm.Api.Infrastructure.Options;

using System.ComponentModel.DataAnnotations;

public class PortalOptions
{
    public const string Key = "Portal";

    [Required]
    public required string BaseUrl { get; set; }

    /// <summary>
    /// Number of days before an invitation link expires. Default is 7 days.
    /// </summary>
    public int InvitationExpiryDays { get; set; } = 7;
}
