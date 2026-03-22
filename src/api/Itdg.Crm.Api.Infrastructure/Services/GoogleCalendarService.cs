namespace Itdg.Crm.Api.Infrastructure.Services;

using System.Diagnostics;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class GoogleCalendarService : IGoogleCalendarService
{
    private const string DefaultEventFields = "id, summary, description, location, start, end, htmlLink, status, organizer, created, updated";
    private const string DefaultListFields = "nextPageToken, items(id, summary, description, location, start, end, htmlLink, status, organizer, created, updated)";

    private readonly GoogleCalendarOptions _options;
    private readonly ILogger<GoogleCalendarService> _logger;

    public GoogleCalendarService(IOptions<GoogleCalendarOptions> options, ILogger<GoogleCalendarService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<CalendarEventListDto> ListEventsAsync(
        string userAccessToken,
        string calendarId = "primary",
        DateTimeOffset? timeMin = null,
        DateTimeOffset? timeMax = null,
        int maxResults = 250,
        string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Google Calendar List Events");
        activity?.SetTag("CalendarId", calendarId);

        using var service = CreateCalendarClient(userAccessToken);

        var request = service.Events.List(calendarId);
        request.MaxResults = maxResults;
        request.Fields = DefaultListFields;
        request.SingleEvents = true;
        request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

        if (timeMin.HasValue)
        {
            request.TimeMinDateTimeOffset = timeMin.Value;
        }

        if (timeMax.HasValue)
        {
            request.TimeMaxDateTimeOffset = timeMax.Value;
        }

        if (!string.IsNullOrWhiteSpace(pageToken))
        {
            request.PageToken = pageToken;
        }

        _logger.LogInformation(
            "Listing Google Calendar events for calendar '{CalendarId}', maxResults {MaxResults}",
            calendarId, maxResults);

        var response = await request.ExecuteAsync(cancellationToken);

        var events = response.Items is not null
            ? response.Items.Select(e => MapToDto(e, calendarId)).ToList().AsReadOnly()
            : (IReadOnlyList<CalendarEventDto>)[];

        return new CalendarEventListDto(events, response.NextPageToken);
    }

    public async Task<CalendarEventListDto> ListEventsFromMultipleCalendarsAsync(
        string userAccessToken,
        IEnumerable<string> calendarIds,
        DateTimeOffset? timeMin = null,
        DateTimeOffset? timeMax = null,
        int maxResultsPerCalendar = 250,
        CancellationToken cancellationToken = default)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Google Calendar List Events Multiple Calendars");

        var calendarIdList = calendarIds.ToList();
        activity?.SetTag("CalendarCount", calendarIdList.Count);

        _logger.LogInformation(
            "Listing Google Calendar events from {CalendarCount} calendars",
            calendarIdList.Count);

        var tasks = calendarIdList.Select(calendarId =>
            ListEventsAsync(userAccessToken, calendarId, timeMin, timeMax, maxResultsPerCalendar, pageToken: null, cancellationToken));

        var results = await Task.WhenAll(tasks);

        var allEvents = results
            .SelectMany(r => r.Events)
            .OrderBy(e => e.Start)
            .ToList()
            .AsReadOnly();

        return new CalendarEventListDto(allEvents, NextPageToken: null);
    }

    public async Task<CalendarEventDto> CreateEventAsync(
        string userAccessToken,
        string calendarId,
        string summary,
        string? description,
        string? location,
        DateTimeOffset start,
        DateTimeOffset end,
        IEnumerable<string>? attendeeEmails = null,
        CancellationToken cancellationToken = default)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Google Calendar Create Event");
        activity?.SetTag("CalendarId", calendarId);
        activity?.SetTag("Summary", summary);

        using var service = CreateCalendarClient(userAccessToken);

        var newEvent = new Event
        {
            Summary = summary,
            Description = description,
            Location = location,
            Start = new EventDateTime { DateTimeDateTimeOffset = start },
            End = new EventDateTime { DateTimeDateTimeOffset = end },
        };

        if (attendeeEmails is not null)
        {
            newEvent.Attendees = attendeeEmails
                .Select(email => new EventAttendee { Email = email })
                .ToList();
        }

        var request = service.Events.Insert(newEvent, calendarId);
        request.Fields = DefaultEventFields;

        _logger.LogInformation(
            "Creating Google Calendar event '{Summary}' in calendar '{CalendarId}'",
            summary, calendarId);

        var createdEvent = await request.ExecuteAsync(cancellationToken);

        return MapToDto(createdEvent, calendarId);
    }

    internal CalendarService CreateCalendarClient(string userAccessToken)
    {
        var credential = GoogleCredential.FromAccessToken(userAccessToken);

        return new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _options.ApplicationName,
        });
    }

    internal static CalendarEventDto MapToDto(Event calendarEvent, string calendarId)
    {
        return new CalendarEventDto(
            Id: calendarEvent.Id,
            Summary: calendarEvent.Summary,
            Description: calendarEvent.Description,
            Location: calendarEvent.Location,
            Start: calendarEvent.Start?.DateTimeDateTimeOffset,
            End: calendarEvent.End?.DateTimeDateTimeOffset,
            CalendarId: calendarId,
            HtmlLink: calendarEvent.HtmlLink,
            Status: calendarEvent.Status,
            OrganizerEmail: calendarEvent.Organizer?.Email,
            Created: calendarEvent.CreatedDateTimeOffset,
            Updated: calendarEvent.UpdatedDateTimeOffset
        );
    }
}
