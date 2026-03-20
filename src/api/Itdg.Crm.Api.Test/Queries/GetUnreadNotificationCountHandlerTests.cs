namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetUnreadNotificationCountHandlerTests
{
    private readonly INotificationRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetUnreadNotificationCountHandler> _logger;
    private readonly GetUnreadNotificationCountHandler _handler;

    public GetUnreadNotificationCountHandlerTests()
    {
        _repository = Substitute.For<INotificationRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _currentUserProvider = Substitute.For<ICurrentUserProvider>();
        _logger = Substitute.For<ILogger<GetUnreadNotificationCountHandler>>();
        _handler = new GetUnreadNotificationCountHandler(_repository, _userRepository, _currentUserProvider, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsUnreadCount_WhenUserExists()
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
        _repository.GetUnreadCountByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(5);

        var query = new GetUnreadNotificationCount();

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public async Task HandleAsync_ReturnsZero_WhenNoEntraObjectId()
    {
        // Arrange
        _currentUserProvider.GetEntraObjectId().Returns((string?)null);

        var query = new GetUnreadNotificationCount();

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_ReturnsZero_WhenUserNotFound()
    {
        // Arrange
        _currentUserProvider.GetEntraObjectId().Returns("unknown-id");
        _userRepository.GetByEntraObjectIdAsync("unknown-id", Arg.Any<CancellationToken>()).Returns((User?)null);

        var query = new GetUnreadNotificationCount();

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().Be(0);
    }
}
