namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetDashboardCalendarHandlerTests
{
    private readonly IGoogleCalendarService _calendarService;
    private readonly IUserIntegrationTokenRepository _tokenRepository;
    private readonly ITokenEncryptionService _tokenEncryptionService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetDashboardCalendarHandler> _logger;
    private readonly GetDashboardCalendarHandler _handler;

    public GetDashboardCalendarHandlerTests()
    {
        _calendarService = Substitute.For<IGoogleCalendarService>();
        _tokenRepository = Substitute.For<IUserIntegrationTokenRepository>();
        _tokenEncryptionService = Substitute.For<ITokenEncryptionService>();
        _userRepository = Substitute.For<IUserRepository>();
        _logger = Substitute.For<ILogger<GetDashboardCalendarHandler>>();
        _handler = new GetDashboardCalendarHandler(
            _calendarService,
            _tokenRepository,
            _tokenEncryptionService,
            _userRepository,
            _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsEvents_FromAllConnectedTeamMembers()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var startDate = DateTimeOffset.UtcNow;
        var endDate = startDate.AddDays(7);

        var tokens = new List<UserIntegrationToken>
        {
            CreateToken(userId1, "Google"),
            CreateToken(userId2, "Google"),
        };
        _tokenRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(tokens);

        var user1 = CreateUser(userId1, "Alice Rivera");
        var user2 = CreateUser(userId2, "Bob Santiago");
        _userRepository.GetByIdAsync(userId1, Arg.Any<CancellationToken>()).Returns(user1);
        _userRepository.GetByIdAsync(userId2, Arg.Any<CancellationToken>()).Returns(user2);

        _tokenEncryptionService.Decrypt(Arg.Any<string>()).Returns("decrypted-token");

        var user1Events = new CalendarEventListDto(
            Events: new List<CalendarEventDto>
            {
                new("evt-1", "Team Meeting", null, null, startDate.AddHours(10), startDate.AddHours(11), "primary", null, "confirmed", null, null, null)
            },
            NextPageToken: null);

        var user2Events = new CalendarEventListDto(
            Events: new List<CalendarEventDto>
            {
                new("evt-2", "Client Call", null, null, startDate.AddHours(14), startDate.AddHours(15), "primary", null, "confirmed", null, null, null)
            },
            NextPageToken: null);

        _calendarService.ListEventsAsync("decrypted-token", "primary", startDate, endDate, 100, null, Arg.Any<CancellationToken>())
            .Returns(user1Events, user2Events);

        // Act
        var result = await _handler.HandleAsync(
            new GetDashboardCalendar(startDate, endDate),
            Guid.NewGuid(),
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Events.Should().HaveCount(2);
        result.TeamMembers.Should().HaveCount(2);
        result.TeamMembers.Should().Contain(m => m.Name == "Alice Rivera");
        result.TeamMembers.Should().Contain(m => m.Name == "Bob Santiago");
        result.Events.Should().Contain(e => e.Summary == "Team Meeting" && e.TeamMemberName == "Alice Rivera");
        result.Events.Should().Contain(e => e.Summary == "Client Call" && e.TeamMemberName == "Bob Santiago");
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyResult_WhenNoGoogleTokensExist()
    {
        // Arrange
        _tokenRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<UserIntegrationToken>());

        // Act
        var result = await _handler.HandleAsync(
            new GetDashboardCalendar(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7)),
            Guid.NewGuid(),
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Events.Should().BeEmpty();
        result.TeamMembers.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_SkipsInactiveUsers()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokens = new List<UserIntegrationToken> { CreateToken(userId, "Google") };
        _tokenRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(tokens);

        var inactiveUser = CreateUser(userId, "Inactive User", isActive: false);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(inactiveUser);

        // Act
        var result = await _handler.HandleAsync(
            new GetDashboardCalendar(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7)),
            Guid.NewGuid(),
            CancellationToken.None);

        // Assert
        result.Events.Should().BeEmpty();
        result.TeamMembers.Should().BeEmpty();
        await _calendarService.DidNotReceive().ListEventsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset?>(),
            Arg.Any<DateTimeOffset?>(), Arg.Any<int>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_SkipsNonGoogleTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokens = new List<UserIntegrationToken> { CreateToken(userId, "Gmail") };
        _tokenRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(tokens);

        // Act
        var result = await _handler.HandleAsync(
            new GetDashboardCalendar(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7)),
            Guid.NewGuid(),
            CancellationToken.None);

        // Assert
        result.Events.Should().BeEmpty();
        result.TeamMembers.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ContinuesOnCalendarServiceFailure()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var startDate = DateTimeOffset.UtcNow;
        var endDate = startDate.AddDays(7);

        var tokens = new List<UserIntegrationToken>
        {
            CreateToken(userId1, "Google"),
            CreateToken(userId2, "Google"),
        };
        _tokenRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(tokens);

        _userRepository.GetByIdAsync(userId1, Arg.Any<CancellationToken>())
            .Returns(CreateUser(userId1, "Alice Rivera"));
        _userRepository.GetByIdAsync(userId2, Arg.Any<CancellationToken>())
            .Returns(CreateUser(userId2, "Bob Santiago"));

        _tokenEncryptionService.Decrypt(Arg.Any<string>()).Returns("decrypted-token");

        // First call throws, second succeeds
        _calendarService.ListEventsAsync("decrypted-token", "primary", startDate, endDate, 100, null, Arg.Any<CancellationToken>())
            .Returns(
                x => throw new InvalidOperationException("Token expired"),
                x => new CalendarEventListDto(
                    Events: new List<CalendarEventDto>
                    {
                        new("evt-1", "Meeting", null, null, startDate.AddHours(10), startDate.AddHours(11), "primary", null, "confirmed", null, null, null)
                    },
                    NextPageToken: null));

        // Act
        var result = await _handler.HandleAsync(
            new GetDashboardCalendar(startDate, endDate),
            Guid.NewGuid(),
            CancellationToken.None);

        // Assert
        result.Events.Should().HaveCount(1);
        result.Events[0].TeamMemberName.Should().Be("Bob Santiago");
        result.TeamMembers.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_SortsEventsByStartTime()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = DateTimeOffset.UtcNow;
        var endDate = startDate.AddDays(7);

        var tokens = new List<UserIntegrationToken> { CreateToken(userId, "Google") };
        _tokenRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(tokens);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(CreateUser(userId, "Alice Rivera"));
        _tokenEncryptionService.Decrypt(Arg.Any<string>()).Returns("decrypted-token");

        var events = new CalendarEventListDto(
            Events: new List<CalendarEventDto>
            {
                new("evt-2", "Late Meeting", null, null, startDate.AddHours(14), startDate.AddHours(15), "primary", null, "confirmed", null, null, null),
                new("evt-1", "Early Meeting", null, null, startDate.AddHours(9), startDate.AddHours(10), "primary", null, "confirmed", null, null, null),
            },
            NextPageToken: null);

        _calendarService.ListEventsAsync("decrypted-token", "primary", startDate, endDate, 100, null, Arg.Any<CancellationToken>())
            .Returns(events);

        // Act
        var result = await _handler.HandleAsync(
            new GetDashboardCalendar(startDate, endDate),
            Guid.NewGuid(),
            CancellationToken.None);

        // Assert
        result.Events.Should().HaveCount(2);
        result.Events[0].Summary.Should().Be("Early Meeting");
        result.Events[1].Summary.Should().Be("Late Meeting");
    }

    [Fact]
    public async Task HandleAsync_AssignsUniqueColorsToTeamMembers()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var tokens = new List<UserIntegrationToken>
        {
            CreateToken(userId1, "Google"),
            CreateToken(userId2, "Google"),
        };
        _tokenRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(tokens);

        _userRepository.GetByIdAsync(userId1, Arg.Any<CancellationToken>())
            .Returns(CreateUser(userId1, "Alice Rivera"));
        _userRepository.GetByIdAsync(userId2, Arg.Any<CancellationToken>())
            .Returns(CreateUser(userId2, "Bob Santiago"));

        _tokenEncryptionService.Decrypt(Arg.Any<string>()).Returns("decrypted-token");

        var emptyEvents = new CalendarEventListDto(Events: new List<CalendarEventDto>(), NextPageToken: null);
        _calendarService.ListEventsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset?>(),
            Arg.Any<DateTimeOffset?>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(emptyEvents);

        // Act
        var result = await _handler.HandleAsync(
            new GetDashboardCalendar(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7)),
            Guid.NewGuid(),
            CancellationToken.None);

        // Assert
        result.TeamMembers.Should().HaveCount(2);
        result.TeamMembers[0].Color.Should().NotBe(result.TeamMembers[1].Color);
    }

    private static UserIntegrationToken CreateToken(Guid userId, string provider)
    {
        return new UserIntegrationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = provider,
            EncryptedAccessToken = "encrypted-token",
            TenantId = Guid.NewGuid(),
        };
    }

    private static User CreateUser(Guid id, string displayName, bool isActive = true)
    {
        return new User
        {
            Id = id,
            EntraObjectId = Guid.NewGuid().ToString(),
            Email = $"{displayName.Replace(" ", ".").ToLowerInvariant()}@example.com",
            DisplayName = displayName,
            Role = Domain.GeneralConstants.UserRole.Associate,
            IsActive = isActive,
            TenantId = Guid.NewGuid(),
        };
    }
}
