namespace Itdg.Crm.Api.Test.Services;

using Itdg.Crm.Api.Infrastructure.Options;
using Itdg.Crm.Api.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GoogleCalendarEvent = Google.Apis.Calendar.v3.Data.Event;
using GoogleEventDateTime = Google.Apis.Calendar.v3.Data.EventDateTime;
using GoogleEventOrganizer = Google.Apis.Calendar.v3.Data.Event.OrganizerData;

public class GoogleCalendarServiceTests
{
    private readonly IOptions<GoogleCalendarOptions> _options;
    private readonly ILogger<GoogleCalendarService> _logger;

    public GoogleCalendarServiceTests()
    {
        var calendarOptions = new GoogleCalendarOptions
        {
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            ApplicationName = "Test-App"
        };

        _options = Microsoft.Extensions.Options.Options.Create(calendarOptions);
        _logger = Substitute.For<ILogger<GoogleCalendarService>>();
    }

    [Fact]
    public void Constructor_InitializesWithOptions()
    {
        // Act
        var service = new GoogleCalendarService(_options, _logger);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void MapToDto_MapsBasicFieldsCorrectly()
    {
        // Arrange
        var calendarEvent = new GoogleCalendarEvent
        {
            Id = "event-123",
            Summary = "Team Meeting",
            Description = "Quarterly planning session",
            Location = "Conference Room A",
            Start = new GoogleEventDateTime
            {
                DateTimeDateTimeOffset = new DateTimeOffset(2025, 7, 1, 10, 0, 0, TimeSpan.Zero)
            },
            End = new GoogleEventDateTime
            {
                DateTimeDateTimeOffset = new DateTimeOffset(2025, 7, 1, 11, 0, 0, TimeSpan.Zero)
            },
            HtmlLink = "https://calendar.google.com/event/event-123",
            Status = "confirmed",
            Organizer = new GoogleEventOrganizer { Email = "admin@example.com" },
            CreatedDateTimeOffset = new DateTimeOffset(2025, 6, 15, 10, 30, 0, TimeSpan.Zero),
            UpdatedDateTimeOffset = new DateTimeOffset(2025, 6, 16, 14, 0, 0, TimeSpan.Zero)
        };

        // Act
        var dto = GoogleCalendarService.MapToDto(calendarEvent, "primary");

        // Assert
        dto.Id.Should().Be("event-123");
        dto.Summary.Should().Be("Team Meeting");
        dto.Description.Should().Be("Quarterly planning session");
        dto.Location.Should().Be("Conference Room A");
        dto.Start.Should().Be(new DateTimeOffset(2025, 7, 1, 10, 0, 0, TimeSpan.Zero));
        dto.End.Should().Be(new DateTimeOffset(2025, 7, 1, 11, 0, 0, TimeSpan.Zero));
        dto.CalendarId.Should().Be("primary");
        dto.HtmlLink.Should().Be("https://calendar.google.com/event/event-123");
        dto.Status.Should().Be("confirmed");
        dto.OrganizerEmail.Should().Be("admin@example.com");
        dto.Created.Should().Be(new DateTimeOffset(2025, 6, 15, 10, 30, 0, TimeSpan.Zero));
        dto.Updated.Should().Be(new DateTimeOffset(2025, 6, 16, 14, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void MapToDto_HandlesNullOptionalFields()
    {
        // Arrange
        var calendarEvent = new GoogleCalendarEvent
        {
            Id = "event-minimal",
            Summary = null,
            Description = null,
            Location = null,
            Start = null,
            End = null,
            HtmlLink = null,
            Status = null,
            Organizer = null,
            CreatedDateTimeOffset = null,
            UpdatedDateTimeOffset = null
        };

        // Act
        var dto = GoogleCalendarService.MapToDto(calendarEvent, "calendar-1");

        // Assert
        dto.Id.Should().Be("event-minimal");
        dto.Summary.Should().BeNull();
        dto.Description.Should().BeNull();
        dto.Location.Should().BeNull();
        dto.Start.Should().BeNull();
        dto.End.Should().BeNull();
        dto.CalendarId.Should().Be("calendar-1");
        dto.HtmlLink.Should().BeNull();
        dto.Status.Should().BeNull();
        dto.OrganizerEmail.Should().BeNull();
        dto.Created.Should().BeNull();
        dto.Updated.Should().BeNull();
    }

    [Fact]
    public void MapToDto_HandlesNullOrganizerEmail()
    {
        // Arrange
        var calendarEvent = new GoogleCalendarEvent
        {
            Id = "event-no-organizer-email",
            Summary = "Meeting",
            Organizer = new GoogleEventOrganizer { Email = null }
        };

        // Act
        var dto = GoogleCalendarService.MapToDto(calendarEvent, "primary");

        // Assert
        dto.OrganizerEmail.Should().BeNull();
    }

    [Fact]
    public void MapToDto_PreservesCalendarId()
    {
        // Arrange
        var calendarEvent = new GoogleCalendarEvent
        {
            Id = "event-456",
            Summary = "Client Call"
        };

        // Act
        var dto = GoogleCalendarService.MapToDto(calendarEvent, "team-calendar@group.calendar.google.com");

        // Assert
        dto.CalendarId.Should().Be("team-calendar@group.calendar.google.com");
    }

    [Fact]
    public void MapToDto_HandlesNullStartEndDateTimeOffset()
    {
        // Arrange — all-day events may have Date but no DateTimeDateTimeOffset
        var calendarEvent = new GoogleCalendarEvent
        {
            Id = "event-allday",
            Summary = "All Day Event",
            Start = new GoogleEventDateTime { DateTimeDateTimeOffset = null },
            End = new GoogleEventDateTime { DateTimeDateTimeOffset = null }
        };

        // Act
        var dto = GoogleCalendarService.MapToDto(calendarEvent, "primary");

        // Assert
        dto.Start.Should().BeNull();
        dto.End.Should().BeNull();
    }

    [Fact]
    public void CreateCalendarClient_ReturnsValidService()
    {
        // Arrange
        var service = new GoogleCalendarService(_options, _logger);

        // Act
        using var calendarClient = service.CreateCalendarClient("test-access-token");

        // Assert
        calendarClient.Should().NotBeNull();
        calendarClient.ApplicationName.Should().Be("Test-App");
    }
}
