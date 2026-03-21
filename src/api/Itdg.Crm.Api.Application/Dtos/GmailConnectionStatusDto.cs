namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record GmailConnectionStatusDto(
    [property: JsonPropertyName("is_connected")] bool IsConnected,
    [property: JsonPropertyName("connected_at")] DateTimeOffset? ConnectedAt
);
