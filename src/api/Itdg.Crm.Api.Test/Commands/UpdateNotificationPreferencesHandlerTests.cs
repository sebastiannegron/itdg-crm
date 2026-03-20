namespace Itdg.Crm.Api.Test.Commands;

using FluentAssertions;
using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;

public class UpdateNotificationPreferencesHandlerTests
{
    private readonly INotificationPreferenceRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<UpdateNotificationPreferencesHandler> _logger;
    private readonly UpdateNotificationPreferencesHandler _handler;

    public UpdateNotificationPreferencesHandlerTests()
    {
        _repository = Substitute.For<INotificationPreferenceRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _currentUserProvider = Substitute.For<ICurrentUserProvider>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _logger = Substitute.For<ILogger<UpdateNotificationPreferencesHandler>>();
        _handler = new UpdateNotificationPreferencesHandler(_repository, _userRepository, _currentUserProvider, _tenantProvider, _logger);
    }

    [Fact]
    public async Task HandleAsync_UpdatesExistingPreference_WhenPreferenceExists()
    {
        // Arrange
        var entraObjectId = "entra-object-id";
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var user = new User { Id = userId, EntraObjectId = entraObjectId, Email = "test@test.com", DisplayName = "Test User", Role = UserRole.Administrator };

        _currentUserProvider.GetEntraObjectId().Returns(entraObjectId);
        _userRepository.GetByEntraObjectIdAsync(entraObjectId, Arg.Any<CancellationToken>()).Returns(user);
        _tenantProvider.GetTenantId().Returns(tenantId);

        var existingPreference = new NotificationPreference
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            EventType = NotificationEventType.TaskAssigned,
            Channel = NotificationChannel.InApp,
            IsEnabled = true,
            DigestMode = "Immediate"
        };

        _repository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(new List<NotificationPreference> { existingPreference });

        var preferences = new List<NotificationPreferenceDto>
        {
            new(Guid.Empty, "TaskAssigned", "InApp", false, "Daily")
        };

        var command = new UpdateNotificationPreferences(preferences);

        // Act
        await _handler.HandleAsync(command, string.Empty, Guid.NewGuid(), CancellationToken.None);

        // Assert
        existingPreference.IsEnabled.Should().BeFalse();
        existingPreference.DigestMode.Should().Be("Daily");
        await _repository.Received(1).UpdateAsync(existingPreference, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_CreatesNewPreference_WhenPreferenceDoesNotExist()
    {
        // Arrange
        var entraObjectId = "entra-object-id";
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var user = new User { Id = userId, EntraObjectId = entraObjectId, Email = "test@test.com", DisplayName = "Test User", Role = UserRole.Administrator };

        _currentUserProvider.GetEntraObjectId().Returns(entraObjectId);
        _userRepository.GetByEntraObjectIdAsync(entraObjectId, Arg.Any<CancellationToken>()).Returns(user);
        _tenantProvider.GetTenantId().Returns(tenantId);

        _repository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(new List<NotificationPreference>());

        var preferences = new List<NotificationPreferenceDto>
        {
            new(Guid.Empty, "TaskAssigned", "Email", true, "Immediate")
        };

        var command = new UpdateNotificationPreferences(preferences);

        // Act
        await _handler.HandleAsync(command, string.Empty, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Is<NotificationPreference>(p =>
            p.UserId == userId &&
            p.TenantId == tenantId &&
            p.EventType == NotificationEventType.TaskAssigned &&
            p.Channel == NotificationChannel.Email &&
            p.IsEnabled == true &&
            p.DigestMode == "Immediate"
        ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DoesNothing_WhenNoEntraObjectId()
    {
        // Arrange
        _currentUserProvider.GetEntraObjectId().Returns((string?)null);

        var command = new UpdateNotificationPreferences(new List<NotificationPreferenceDto>());

        // Act
        await _handler.HandleAsync(command, string.Empty, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_SkipsInvalidEventTypeOrChannel()
    {
        // Arrange
        var entraObjectId = "entra-object-id";
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, EntraObjectId = entraObjectId, Email = "test@test.com", DisplayName = "Test User", Role = UserRole.Administrator };

        _currentUserProvider.GetEntraObjectId().Returns(entraObjectId);
        _userRepository.GetByEntraObjectIdAsync(entraObjectId, Arg.Any<CancellationToken>()).Returns(user);

        _repository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(new List<NotificationPreference>());

        var preferences = new List<NotificationPreferenceDto>
        {
            new(Guid.Empty, "InvalidEvent", "InApp", true, "Immediate")
        };

        var command = new UpdateNotificationPreferences(preferences);

        // Act
        await _handler.HandleAsync(command, string.Empty, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.DidNotReceive().AddAsync(Arg.Any<NotificationPreference>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<NotificationPreference>(), Arg.Any<CancellationToken>());
    }
}
