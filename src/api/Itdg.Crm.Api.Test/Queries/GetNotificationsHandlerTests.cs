namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetNotificationsHandlerTests
{
    private readonly INotificationRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetNotificationsHandler> _logger;
    private readonly GetNotificationsHandler _handler;

    public GetNotificationsHandlerTests()
    {
        _repository = Substitute.For<INotificationRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _currentUserProvider = Substitute.For<ICurrentUserProvider>();
        _logger = Substitute.For<ILogger<GetNotificationsHandler>>();
        _handler = new GetNotificationsHandler(_repository, _userRepository, _currentUserProvider, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsPaginatedNotifications_WhenUserExists()
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

        var notifications = new List<Notification>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EventType = NotificationEventType.DocumentUploaded,
                Channel = NotificationChannel.InApp,
                Title = "Doc uploaded",
                Body = "A document was uploaded",
                Status = NotificationStatus.Delivered,
                TenantId = user.TenantId,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        _currentUserProvider.GetEntraObjectId().Returns(entraObjectId);
        _userRepository.GetByEntraObjectIdAsync(entraObjectId, Arg.Any<CancellationToken>()).Returns(user);
        _repository.GetPagedByUserIdAsync(userId, 1, 20, null, Arg.Any<CancellationToken>())
            .Returns((notifications.AsReadOnly(), 1));

        var query = new GetNotifications();

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.Items[0].Title.Should().Be("Doc uploaded");
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyResult_WhenUserNotFound()
    {
        // Arrange
        _currentUserProvider.GetEntraObjectId().Returns("unknown-id");
        _userRepository.GetByEntraObjectIdAsync("unknown-id", Arg.Any<CancellationToken>()).Returns((User?)null);

        var query = new GetNotifications();

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyResult_WhenNoEntraObjectId()
    {
        // Arrange
        _currentUserProvider.GetEntraObjectId().Returns((string?)null);

        var query = new GetNotifications();

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_FiltersByStatus_WhenStatusProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entraObjectId = "entra-456";
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
        _repository.GetPagedByUserIdAsync(userId, 1, 20, NotificationStatus.Read, Arg.Any<CancellationToken>())
            .Returns((new List<Notification>().AsReadOnly(), 0));

        var query = new GetNotifications(Status: NotificationStatus.Read);

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).GetPagedByUserIdAsync(userId, 1, 20, NotificationStatus.Read, Arg.Any<CancellationToken>());
    }
}
