namespace Itdg.Crm.Api.Requests;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Itdg.Crm.Api.Domain.Enums;

public class CreateTemplateRequest
{
    [JsonPropertyName("category")]
    [Required]
    public required TemplateCategory Category { get; set; }

    [JsonPropertyName("name")]
    [Required, StringLength(200, MinimumLength = 2)]
    public required string Name { get; set; }

    [JsonPropertyName("subject_template")]
    [Required, StringLength(500, MinimumLength = 2)]
    public required string SubjectTemplate { get; set; }

    [JsonPropertyName("body_template")]
    [Required]
    public required string BodyTemplate { get; set; }

    [JsonPropertyName("language")]
    [Required, StringLength(10, MinimumLength = 2)]
    public required string Language { get; set; }
}
