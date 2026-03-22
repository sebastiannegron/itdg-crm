namespace Itdg.Crm.Api.Infrastructure.Options;

using System.ComponentModel.DataAnnotations;

public class AzureSearchOptions
{
    public const string Key = "AzureAiSearch";

    [Required]
    public required string Endpoint { get; set; }

    [Required]
    public required string ApiKey { get; set; }

    [Required]
    public required string IndexName { get; set; }
}
