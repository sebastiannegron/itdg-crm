namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record DashboardCalendarEventDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("summary")] string? Summary,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("start")] DateTimeOffset? Start,
    [property: JsonPropertyName("end")] DateTimeOffset? End,
    [property: JsonPropertyName("calendar_id")] string CalendarId,
    [property: JsonPropertyName("html_link")] string? HtmlLink,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("organizer_email")] string? OrganizerEmail,
    [property: JsonPropertyName("created")] DateTimeOffset? Created,
    [property: JsonPropertyName("updated")] DateTimeOffset? Updated,
    [property: JsonPropertyName("team_member_name")] string TeamMemberName,
    [property: JsonPropertyName("team_member_color")] string TeamMemberColor
);
