namespace Itdg.Crm.Api.Infrastructure.Options;

using System.ComponentModel.DataAnnotations;

public class MicrosoftGraphEmailOptions
{
    public const string Key = "MicrosoftGraphEmail";

    [Required]
    public required string TenantId { get; set; }

    [Required]
    public required string ClientId { get; set; }

    [Required]
    public required string ClientSecret { get; set; }

    [Required]
    [EmailAddress]
    public required string SenderAddress { get; set; }
}
