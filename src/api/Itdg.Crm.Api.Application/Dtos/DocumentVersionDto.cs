namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record DocumentVersionDto(
    [property: JsonPropertyName("version_id")] Guid VersionId,
    [property: JsonPropertyName("document_id")] Guid DocumentId,
    [property: JsonPropertyName("version_number")] int VersionNumber,
    [property: JsonPropertyName("google_drive_file_id")] string GoogleDriveFileId,
    [property: JsonPropertyName("uploaded_by_id")] Guid UploadedById,
    [property: JsonPropertyName("uploaded_at")] DateTimeOffset UploadedAt
);
