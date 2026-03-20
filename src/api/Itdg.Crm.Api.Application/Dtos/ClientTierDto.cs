namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record ClientTierDto(
    [property: JsonPropertyName("tier_id")] Guid TierId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("sort_order")] int SortOrder,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt
);
