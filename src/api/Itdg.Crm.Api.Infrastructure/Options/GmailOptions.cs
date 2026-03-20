namespace Itdg.Crm.Api.Infrastructure.Options;

using System.ComponentModel.DataAnnotations;

public class GmailOptions
{
    public const string Key = "Gmail";

    [Required]
    public required string ClientId { get; set; }

    [Required]
    public required string ClientSecret { get; set; }

    public string ApplicationName { get; set; } = "ITDG-CRM";
}
