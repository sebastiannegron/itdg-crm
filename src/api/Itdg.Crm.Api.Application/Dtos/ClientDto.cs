namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record ClientDto(
    [property: JsonPropertyName("client_id")] Guid ClientId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("contact_email")] string? ContactEmail,
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("address")] string? Address,
    [property: JsonPropertyName("tier_id")] Guid? TierId,
    [property: JsonPropertyName("tier_name")] string? TierName,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("industry_tag")] string? IndustryTag,
    [property: JsonPropertyName("notes")] string? Notes,
    [property: JsonPropertyName("custom_fields")] string? CustomFields,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt
);
