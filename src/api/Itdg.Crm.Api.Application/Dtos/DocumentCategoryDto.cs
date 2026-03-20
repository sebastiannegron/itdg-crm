namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record DocumentCategoryDto(
    [property: JsonPropertyName("category_id")] Guid CategoryId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("naming_convention")] string? NamingConvention,
    [property: JsonPropertyName("is_default")] bool IsDefault,
    [property: JsonPropertyName("sort_order")] int SortOrder,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt
);
