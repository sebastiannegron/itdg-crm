namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record CalendarEventListDto(
    [property: JsonPropertyName("events")] IReadOnlyList<CalendarEventDto> Events,
    [property: JsonPropertyName("next_page_token")] string? NextPageToken
);
