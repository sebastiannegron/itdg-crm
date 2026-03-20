namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record NotificationDto(
    [property: JsonPropertyName("notification_id")] Guid NotificationId,
    [property: JsonPropertyName("user_id")] Guid UserId,
    [property: JsonPropertyName("event_type")] string EventType,
    [property: JsonPropertyName("channel")] string Channel,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("delivered_at")] DateTimeOffset? DeliveredAt,
    [property: JsonPropertyName("read_at")] DateTimeOffset? ReadAt,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt
);
