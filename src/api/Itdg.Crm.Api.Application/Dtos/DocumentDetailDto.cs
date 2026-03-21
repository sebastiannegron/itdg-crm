namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record DocumentDetailDto(
    [property: JsonPropertyName("document_id")] Guid DocumentId,
    [property: JsonPropertyName("client_id")] Guid ClientId,
    [property: JsonPropertyName("category_id")] Guid CategoryId,
    [property: JsonPropertyName("category_name")] string? CategoryName,
    [property: JsonPropertyName("file_name")] string FileName,
    [property: JsonPropertyName("google_drive_file_id")] string GoogleDriveFileId,
    [property: JsonPropertyName("uploaded_by_id")] Guid UploadedById,
    [property: JsonPropertyName("current_version")] int CurrentVersion,
    [property: JsonPropertyName("file_size")] long FileSize,
    [property: JsonPropertyName("mime_type")] string MimeType,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt,
    [property: JsonPropertyName("web_view_link")] string? WebViewLink,
    [property: JsonPropertyName("versions")] IReadOnlyList<DocumentVersionDto> Versions
);
