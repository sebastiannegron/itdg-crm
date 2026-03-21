namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetGmailConnectionStatusHandlerTests
{
    private readonly IUserIntegrationTokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetGmailConnectionStatusHandler> _logger;
    private readonly GetGmailConnectionStatusHandler _handler;

    public GetGmailConnectionStatusHandlerTests()
    {
        _tokenRepository = Substitute.For<IUserIntegrationTokenRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _currentUserProvider = Substitute.For<ICurrentUserProvider>();
        _logger = Substitute.For<ILogger<GetGmailConnectionStatusHandler>>();
        _handler = new GetGmailConnectionStatusHandler(_tokenRepository, _userRepository, _currentUserProvider, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsConnected_WhenTokenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-5);

        _currentUserProvider.GetEntraObjectId().Returns("entra-123");
        _userRepository.GetByEntraObjectIdAsync("entra-123", Arg.Any<CancellationToken>())
            .Returns(new User
            {
                Id = userId,
                EntraObjectId = "entra-123",
                Email = "user@example.com",
                DisplayName = "Test User",
                Role = UserRole.Associate,
                TenantId = Guid.NewGuid()
            });
        _tokenRepository.GetByUserIdAndProviderAsync(userId, "Gmail", Arg.Any<CancellationToken>())
            .Returns(new UserIntegrationToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Provider = "Gmail",
                EncryptedAccessToken = "encrypted",
                TenantId = Guid.NewGuid(),
                CreatedAt = createdAt
            });

        // Act
        var result = await _handler.HandleAsync(new GetGmailConnectionStatus(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.IsConnected.Should().BeTrue();
        result.ConnectedAt.Should().Be(createdAt);
    }

    [Fact]
    public async Task HandleAsync_ReturnsNotConnected_WhenNoToken()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _currentUserProvider.GetEntraObjectId().Returns("entra-123");
        _userRepository.GetByEntraObjectIdAsync("entra-123", Arg.Any<CancellationToken>())
            .Returns(new User
            {
                Id = userId,
                EntraObjectId = "entra-123",
                Email = "user@example.com",
                DisplayName = "Test User",
                Role = UserRole.Associate,
                TenantId = Guid.NewGuid()
            });
        _tokenRepository.GetByUserIdAndProviderAsync(userId, "Gmail", Arg.Any<CancellationToken>())
            .Returns((UserIntegrationToken?)null);

        // Act
        var result = await _handler.HandleAsync(new GetGmailConnectionStatus(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.IsConnected.Should().BeFalse();
        result.ConnectedAt.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ReturnsNotConnected_WhenUserNotFound()
    {
        // Arrange
        _currentUserProvider.GetEntraObjectId().Returns("entra-999");
        _userRepository.GetByEntraObjectIdAsync("entra-999", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _handler.HandleAsync(new GetGmailConnectionStatus(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.IsConnected.Should().BeFalse();
        result.ConnectedAt.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ReturnsNotConnected_WhenEntraObjectIdIsNull()
    {
        // Arrange
        _currentUserProvider.GetEntraObjectId().Returns((string?)null);

        // Act
        var result = await _handler.HandleAsync(new GetGmailConnectionStatus(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.IsConnected.Should().BeFalse();
        result.ConnectedAt.Should().BeNull();
    }
}
