namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record NotificationPreferenceDto(
    [property: JsonPropertyName("preference_id")] Guid PreferenceId,
    [property: JsonPropertyName("event_type")] string EventType,
    [property: JsonPropertyName("channel")] string Channel,
    [property: JsonPropertyName("is_enabled")] bool IsEnabled,
    [property: JsonPropertyName("digest_mode")] string DigestMode
);
