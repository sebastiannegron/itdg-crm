namespace Itdg.Crm.Api.Requests;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class UpdateTierRequest
{
    [JsonPropertyName("name")]
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; set; }

    [JsonPropertyName("sort_order")]
    [Required]
    public required int SortOrder { get; set; }
}
