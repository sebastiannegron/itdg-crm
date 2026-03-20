namespace Itdg.Crm.Api.Requests;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class UpdateNotificationPreferencesRequest
{
    [JsonPropertyName("preferences")]
    [Required]
    public required List<NotificationPreferenceItem> Preferences { get; set; }
}

public class NotificationPreferenceItem
{
    [JsonPropertyName("event_type")]
    [Required, StringLength(50)]
    public required string EventType { get; set; }

    [JsonPropertyName("channel")]
    [Required, StringLength(20)]
    public required string Channel { get; set; }

    [JsonPropertyName("is_enabled")]
    [Required]
    public required bool IsEnabled { get; set; }

    [JsonPropertyName("digest_mode")]
    [Required, StringLength(20)]
    public required string DigestMode { get; set; }
}
