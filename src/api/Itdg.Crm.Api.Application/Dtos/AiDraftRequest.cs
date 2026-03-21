namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record AiDraftRequest(
    [property: JsonPropertyName("client_name")] string ClientName,
    [property: JsonPropertyName("topic")] string Topic,
    [property: JsonPropertyName("language")] string Language,
    [property: JsonPropertyName("additional_context")] string? AdditionalContext = null);
