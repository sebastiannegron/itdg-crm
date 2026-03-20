namespace Itdg.Crm.Api.Test.Queries;

using FluentAssertions;
using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;

public class GetNotificationPreferencesHandlerTests
{
    private readonly INotificationPreferenceRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetNotificationPreferencesHandler> _logger;
    private readonly GetNotificationPreferencesHandler _handler;

    public GetNotificationPreferencesHandlerTests()
    {
        _repository = Substitute.For<INotificationPreferenceRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _currentUserProvider = Substitute.For<ICurrentUserProvider>();
        _logger = Substitute.For<ILogger<GetNotificationPreferencesHandler>>();
        _handler = new GetNotificationPreferencesHandler(_repository, _userRepository, _currentUserProvider, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsPreferences_WhenUserExists()
    {
        // Arrange
        var entraObjectId = "entra-object-id";
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, EntraObjectId = entraObjectId, Email = "test@test.com", DisplayName = "Test User", Role = UserRole.Administrator };

        _currentUserProvider.GetEntraObjectId().Returns(entraObjectId);
        _userRepository.GetByEntraObjectIdAsync(entraObjectId, Arg.Any<CancellationToken>()).Returns(user);

        var preferences = new List<NotificationPreference>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, EventType = NotificationEventType.TaskAssigned, Channel = NotificationChannel.InApp, IsEnabled = true, DigestMode = "Immediate" },
            new() { Id = Guid.NewGuid(), UserId = userId, EventType = NotificationEventType.TaskAssigned, Channel = NotificationChannel.Email, IsEnabled = false, DigestMode = "Daily" }
        };

        _repository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(preferences);

        // Act
        var result = await _handler.HandleAsync(new GetNotificationPreferences(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList[0].EventType.Should().Be("TaskAssigned");
        resultList[0].Channel.Should().Be("InApp");
        resultList[0].IsEnabled.Should().BeTrue();
        resultList[0].DigestMode.Should().Be("Immediate");
        resultList[1].Channel.Should().Be("Email");
        resultList[1].IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmpty_WhenNoEntraObjectId()
    {
        // Arrange
        _currentUserProvider.GetEntraObjectId().Returns((string?)null);

        // Act
        var result = await _handler.HandleAsync(new GetNotificationPreferences(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmpty_WhenUserNotFound()
    {
        // Arrange
        _currentUserProvider.GetEntraObjectId().Returns("entra-object-id");
        _userRepository.GetByEntraObjectIdAsync("entra-object-id", Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _handler.HandleAsync(new GetNotificationPreferences(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
