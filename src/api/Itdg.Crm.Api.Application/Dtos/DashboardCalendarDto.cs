namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record DashboardCalendarDto(
    [property: JsonPropertyName("events")] IReadOnlyList<DashboardCalendarEventDto> Events,
    [property: JsonPropertyName("team_members")] IReadOnlyList<CalendarTeamMemberDto> TeamMembers
);

public record CalendarTeamMemberDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("color")] string Color
);
