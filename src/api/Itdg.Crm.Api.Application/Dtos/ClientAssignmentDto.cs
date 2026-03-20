namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record ClientAssignmentDto(
    [property: JsonPropertyName("user_id")] Guid UserId,
    [property: JsonPropertyName("display_name")] string DisplayName,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("assigned_at")] DateTimeOffset AssignedAt
);
