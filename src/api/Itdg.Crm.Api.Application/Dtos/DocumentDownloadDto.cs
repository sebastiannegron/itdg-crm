namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record DocumentDownloadDto(
    [property: JsonPropertyName("document_id")] Guid DocumentId,
    [property: JsonPropertyName("file_name")] string FileName,
    [property: JsonPropertyName("mime_type")] string MimeType,
    [property: JsonPropertyName("file_size")] long FileSize,
    [property: JsonPropertyName("google_drive_file_id")] string GoogleDriveFileId,
    [property: JsonPropertyName("web_view_link")] string? WebViewLink
);
