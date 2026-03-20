namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record DriveFileListDto(
    [property: JsonPropertyName("files")] IReadOnlyList<DriveFileDto> Files,
    [property: JsonPropertyName("next_page_token")] string? NextPageToken
);
