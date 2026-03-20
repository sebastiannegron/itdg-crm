namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record UserDto(
    [property: JsonPropertyName("user_id")] Guid UserId,
    [property: JsonPropertyName("entra_object_id")] string EntraObjectId,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("display_name")] string DisplayName,
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("is_active")] bool IsActive,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt
);
