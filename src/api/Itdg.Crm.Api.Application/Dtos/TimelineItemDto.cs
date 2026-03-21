namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record TimelineItemDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("timestamp")] DateTimeOffset Timestamp,
    [property: JsonPropertyName("actor")] string? Actor
);
