namespace Itdg.Crm.Api.Infrastructure.Options;

using System.ComponentModel.DataAnnotations;

public class AzureAdOptions
{
    public const string Key = "AzureAd";

    [Required]
    public required string Instance { get; set; }

    [Required]
    public required string TenantId { get; set; }

    [Required]
    public required string ClientId { get; set; }

    [Required]
    public required string Audience { get; set; }
}
