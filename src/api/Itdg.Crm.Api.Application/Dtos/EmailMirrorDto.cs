namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record EmailMirrorDto(
    [property: JsonPropertyName("email_id")] Guid EmailId,
    [property: JsonPropertyName("client_id")] Guid ClientId,
    [property: JsonPropertyName("gmail_message_id")] string GmailMessageId,
    [property: JsonPropertyName("gmail_thread_id")] string GmailThreadId,
    [property: JsonPropertyName("subject")] string Subject,
    [property: JsonPropertyName("from")] string From,
    [property: JsonPropertyName("to")] string To,
    [property: JsonPropertyName("body_preview")] string? BodyPreview,
    [property: JsonPropertyName("has_attachments")] bool HasAttachments,
    [property: JsonPropertyName("received_at")] DateTimeOffset ReceivedAt
);
