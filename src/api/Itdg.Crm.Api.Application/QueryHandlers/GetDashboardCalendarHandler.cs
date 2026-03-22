namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetDashboardCalendarHandler : IQueryHandler<GetDashboardCalendar, DashboardCalendarDto>
{
    private static readonly string[] TeamColors =
    [
        "#3B82F6", // blue
        "#10B981", // emerald
        "#F59E0B", // amber
        "#EF4444", // red
        "#8B5CF6", // violet
        "#EC4899", // pink
        "#06B6D4", // cyan
        "#F97316", // orange
        "#14B8A6", // teal
        "#6366F1", // indigo
    ];

    private readonly IGoogleCalendarService _calendarService;
    private readonly IUserIntegrationTokenRepository _tokenRepository;
    private readonly ITokenEncryptionService _tokenEncryptionService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetDashboardCalendarHandler> _logger;

    public GetDashboardCalendarHandler(
        IGoogleCalendarService calendarService,
        IUserIntegrationTokenRepository tokenRepository,
        ITokenEncryptionService tokenEncryptionService,
        IUserRepository userRepository,
        ILogger<GetDashboardCalendarHandler> logger)
    {
        _calendarService = calendarService;
        _tokenRepository = tokenRepository;
        _tokenEncryptionService = tokenEncryptionService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<DashboardCalendarDto> HandleAsync(GetDashboardCalendar query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Dashboard Calendar");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation(
            "Getting dashboard calendar events from {StartDate} to {EndDate} | CorrelationId: {CorrelationId}",
            query.StartDate, query.EndDate, correlationId);

        var allTokens = await _tokenRepository.GetAllAsync(cancellationToken);
        var googleCalendarTokens = allTokens.Where(t => t.Provider == "Google").ToList();

        var events = new List<DashboardCalendarEventDto>();
        var teamMembers = new List<CalendarTeamMemberDto>();
        var colorIndex = 0;

        foreach (var token in googleCalendarTokens)
        {
            var user = await _userRepository.GetByIdAsync(token.UserId, cancellationToken);
            if (user is null || !user.IsActive)
            {
                continue;
            }

            var color = TeamColors[colorIndex % TeamColors.Length];
            colorIndex++;

            teamMembers.Add(new CalendarTeamMemberDto(user.DisplayName, color));

            try
            {
                var accessToken = _tokenEncryptionService.Decrypt(token.EncryptedAccessToken);
                var calendarEvents = await _calendarService.ListEventsAsync(
                    accessToken,
                    "primary",
                    query.StartDate,
                    query.EndDate,
                    maxResults: 100,
                    cancellationToken: cancellationToken);

                foreach (var calEvent in calendarEvents.Events)
                {
                    events.Add(new DashboardCalendarEventDto(
                        Id: calEvent.Id,
                        Summary: calEvent.Summary,
                        Description: calEvent.Description,
                        Location: calEvent.Location,
                        Start: calEvent.Start,
                        End: calEvent.End,
                        CalendarId: calEvent.CalendarId,
                        HtmlLink: calEvent.HtmlLink,
                        Status: calEvent.Status,
                        OrganizerEmail: calEvent.OrganizerEmail,
                        Created: calEvent.Created,
                        Updated: calEvent.Updated,
                        TeamMemberName: user.DisplayName,
                        TeamMemberColor: color
                    ));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to fetch calendar events for user {UserId} | CorrelationId: {CorrelationId}",
                    user.Id, correlationId);
            }
        }

        var sortedEvents = events.OrderBy(e => e.Start).ToList();

        return new DashboardCalendarDto(sortedEvents, teamMembers);
    }
}
