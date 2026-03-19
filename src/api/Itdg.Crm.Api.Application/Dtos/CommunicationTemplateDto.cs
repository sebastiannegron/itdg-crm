namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;
using Itdg.Crm.Api.Domain.Enums;

public record CommunicationTemplateDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("category")] TemplateCategory Category,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("subject_template")] string SubjectTemplate,
    [property: JsonPropertyName("body_template")] string BodyTemplate,
    [property: JsonPropertyName("language")] string Language,
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("is_active")] bool IsActive,
    [property: JsonPropertyName("created_by_id")] Guid CreatedById,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt
);
