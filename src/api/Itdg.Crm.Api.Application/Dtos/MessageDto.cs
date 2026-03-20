namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record MessageDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("client_id")] Guid ClientId,
    [property: JsonPropertyName("sender_id")] Guid SenderId,
    [property: JsonPropertyName("direction")] string Direction,
    [property: JsonPropertyName("subject")] string Subject,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("template_id")] Guid? TemplateId,
    [property: JsonPropertyName("is_portal_message")] bool IsPortalMessage,
    [property: JsonPropertyName("is_read")] bool IsRead,
    [property: JsonPropertyName("attachments")] string? Attachments,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);
