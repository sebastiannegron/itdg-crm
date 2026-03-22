namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record SearchDocumentDto(
    [property: JsonPropertyName("document_id")] Guid DocumentId,
    [property: JsonPropertyName("client_id")] Guid ClientId,
    [property: JsonPropertyName("client_name")] string ClientName,
    [property: JsonPropertyName("file_name")] string FileName,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("content")] string? Content,
    [property: JsonPropertyName("uploaded_at")] DateTimeOffset UploadedAt
);
