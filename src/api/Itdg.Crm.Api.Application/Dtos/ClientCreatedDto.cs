namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record ClientCreatedDto(
    [property: JsonPropertyName("client_id")] Guid ClientId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt
);
