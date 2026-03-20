namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class MarkAllNotificationsAsReadHandlerTests
{
    private readonly INotificationRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<MarkAllNotificationsAsReadHandler> _logger;
    private readonly MarkAllNotificationsAsReadHandler _handler;

    public MarkAllNotificationsAsReadHandlerTests()
    {
        _repository = Substitute.For<INotificationRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _currentUserProvider = Substitute.For<ICurrentUserProvider>();
        _logger = Substitute.For<ILogger<MarkAllNotificationsAsReadHandler>>();
        _handler = new MarkAllNotificationsAsReadHandler(_repository, _userRepository, _currentUserProvider, _logger);
    }

    [Fact]
    public async Task HandleAsync_MarksAllNotificationsAsRead_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entraObjectId = "entra-123";
        var user = new User
        {
            Id = userId,
            EntraObjectId = entraObjectId,
            DisplayName = "Test User",
            Email = "test@example.com",
            Role = UserRole.Administrator,
            TenantId = Guid.NewGuid()
        };

        _currentUserProvider.GetEntraObjectId().Returns(entraObjectId);
        _userRepository.GetByEntraObjectIdAsync(entraObjectId, Arg.Any<CancellationToken>()).Returns(user);
        _repository.MarkAllAsReadByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var command = new MarkAllNotificationsAsRead();

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).MarkAllAsReadByUserIdAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DoesNothing_WhenNoEntraObjectId()
    {
        // Arrange
        _currentUserProvider.GetEntraObjectId().Returns((string?)null);

        var command = new MarkAllNotificationsAsRead();

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.DidNotReceive().MarkAllAsReadByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DoesNothing_WhenUserNotFound()
    {
        // Arrange
        _currentUserProvider.GetEntraObjectId().Returns("unknown-id");
        _userRepository.GetByEntraObjectIdAsync("unknown-id", Arg.Any<CancellationToken>()).Returns((User?)null);

        var command = new MarkAllNotificationsAsRead();

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.DidNotReceive().MarkAllAsReadByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
