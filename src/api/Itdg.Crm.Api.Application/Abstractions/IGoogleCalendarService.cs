namespace Itdg.Crm.Api.Application.Abstractions;

using Itdg.Crm.Api.Application.Dtos;

/// <summary>
/// Provides Google Calendar API operations using per-user OAuth 2.0 tokens.
/// </summary>
public interface IGoogleCalendarService
{
    /// <summary>
    /// Lists events from a single calendar.
    /// </summary>
    /// <param name="userAccessToken">The user's OAuth 2.0 access token.</param>
    /// <param name="calendarId">The calendar ID (use "primary" for the user's main calendar).</param>
    /// <param name="timeMin">Optional start of the time range to filter events.</param>
    /// <param name="timeMax">Optional end of the time range to filter events.</param>
    /// <param name="maxResults">Maximum number of events to return (default 250).</param>
    /// <param name="pageToken">Token for pagination of results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of calendar events.</returns>
    Task<CalendarEventListDto> ListEventsAsync(
        string userAccessToken,
        string calendarId = "primary",
        DateTimeOffset? timeMin = null,
        DateTimeOffset? timeMax = null,
        int maxResults = 250,
        string? pageToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists events aggregated from multiple calendars.
    /// </summary>
    /// <param name="userAccessToken">The user's OAuth 2.0 access token.</param>
    /// <param name="calendarIds">The calendar IDs to aggregate events from.</param>
    /// <param name="timeMin">Optional start of the time range to filter events.</param>
    /// <param name="timeMax">Optional end of the time range to filter events.</param>
    /// <param name="maxResultsPerCalendar">Maximum number of events to return per calendar (default 250).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A combined list of calendar events sorted by start time.</returns>
    Task<CalendarEventListDto> ListEventsFromMultipleCalendarsAsync(
        string userAccessToken,
        IEnumerable<string> calendarIds,
        DateTimeOffset? timeMin = null,
        DateTimeOffset? timeMax = null,
        int maxResultsPerCalendar = 250,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an event in a calendar.
    /// </summary>
    /// <param name="userAccessToken">The user's OAuth 2.0 access token.</param>
    /// <param name="calendarId">The calendar ID (use "primary" for the user's main calendar).</param>
    /// <param name="summary">The event title/summary.</param>
    /// <param name="description">Optional event description.</param>
    /// <param name="location">Optional event location.</param>
    /// <param name="start">The event start time.</param>
    /// <param name="end">The event end time.</param>
    /// <param name="attendeeEmails">Optional list of attendee email addresses.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created calendar event.</returns>
    Task<CalendarEventDto> CreateEventAsync(
        string userAccessToken,
        string calendarId,
        string summary,
        string? description,
        string? location,
        DateTimeOffset start,
        DateTimeOffset end,
        IEnumerable<string>? attendeeEmails = null,
        CancellationToken cancellationToken = default);
}
