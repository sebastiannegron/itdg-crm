namespace Itdg.Crm.Api.Requests;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class DraftEmailRequest
{
    [JsonPropertyName("client_name")]
    [Required, StringLength(200, MinimumLength = 1)]
    public required string ClientName { get; set; }

    [JsonPropertyName("topic")]
    [Required, StringLength(500, MinimumLength = 1)]
    public required string Topic { get; set; }

    [JsonPropertyName("language")]
    [Required, StringLength(10, MinimumLength = 2)]
    public required string Language { get; set; }

    [JsonPropertyName("additional_context")]
    [StringLength(2000)]
    public string? AdditionalContext { get; set; }
}
