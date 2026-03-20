namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record GmailMessageListDto(
    [property: JsonPropertyName("messages")] IReadOnlyList<GmailMessageDto> Messages,
    [property: JsonPropertyName("next_page_token")] string? NextPageToken,
    [property: JsonPropertyName("result_size_estimate")] int ResultSizeEstimate
);
