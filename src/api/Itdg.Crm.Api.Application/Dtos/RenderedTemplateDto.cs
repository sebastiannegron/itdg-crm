namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record RenderedTemplateDto(
    [property: JsonPropertyName("subject")] string Subject,
    [property: JsonPropertyName("body")] string Body
);
