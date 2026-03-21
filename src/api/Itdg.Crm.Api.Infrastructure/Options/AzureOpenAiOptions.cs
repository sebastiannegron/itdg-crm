namespace Itdg.Crm.Api.Infrastructure.Options;

using System.ComponentModel.DataAnnotations;

public class AzureOpenAiOptions
{
    public const string Key = "AzureOpenAi";

    [Required]
    public required string Endpoint { get; set; }

    [Required]
    public required string ApiKey { get; set; }

    [Required]
    public required string DeploymentName { get; set; }
}
