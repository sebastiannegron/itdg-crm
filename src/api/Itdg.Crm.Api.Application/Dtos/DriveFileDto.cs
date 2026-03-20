namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record DriveFileDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("mime_type")] string MimeType,
    [property: JsonPropertyName("size")] long? Size,
    [property: JsonPropertyName("created_time")] DateTimeOffset? CreatedTime,
    [property: JsonPropertyName("modified_time")] DateTimeOffset? ModifiedTime,
    [property: JsonPropertyName("web_view_link")] string? WebViewLink,
    [property: JsonPropertyName("parents")] IReadOnlyList<string> Parents
);
