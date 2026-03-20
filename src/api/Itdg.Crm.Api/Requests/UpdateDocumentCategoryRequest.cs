namespace Itdg.Crm.Api.Requests;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class UpdateDocumentCategoryRequest
{
    [JsonPropertyName("name")]
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; set; }

    [JsonPropertyName("naming_convention")]
    [StringLength(200)]
    public string? NamingConvention { get; set; }

    [JsonPropertyName("sort_order")]
    [Required]
    public required int SortOrder { get; set; }
}
