namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record GmailMessageDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("thread_id")] string ThreadId,
    [property: JsonPropertyName("subject")] string Subject,
    [property: JsonPropertyName("from")] string From,
    [property: JsonPropertyName("to")] string To,
    [property: JsonPropertyName("snippet")] string Snippet,
    [property: JsonPropertyName("body_preview")] string BodyPreview,
    [property: JsonPropertyName("has_attachments")] bool HasAttachments,
    [property: JsonPropertyName("date")] DateTimeOffset Date,
    [property: JsonPropertyName("label_ids")] IReadOnlyList<string> LabelIds
);
