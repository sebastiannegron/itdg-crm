namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetGoogleConnectionStatusHandlerTests
{
    private readonly IUserIntegrationTokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetGoogleConnectionStatusHandler> _logger;
    private readonly GetGoogleConnectionStatusHandler _handler;

    public GetGoogleConnectionStatusHandlerTests()
    {
        _tokenRepository = Substitute.For<IUserIntegrationTokenRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _currentUserProvider = Substitute.For<ICurrentUserProvider>();
        _logger = Substitute.For<ILogger<GetGoogleConnectionStatusHandler>>();
        _handler = new GetGoogleConnectionStatusHandler(_tokenRepository, _userRepository, _currentUserProvider, _logger);
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
                Email = "admin@example.com",
                DisplayName = "Admin User",
                Role = UserRole.Administrator,
                TenantId = Guid.NewGuid()
            });
        _tokenRepository.GetByUserIdAndProviderAsync(userId, "Google", Arg.Any<CancellationToken>())
            .Returns(new UserIntegrationToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Provider = "Google",
                EncryptedAccessToken = "encrypted",
                TenantId = Guid.NewGuid(),
                CreatedAt = createdAt
            });

        // Act
        var result = await _handler.HandleAsync(new GetGoogleConnectionStatus(), Guid.NewGuid(), CancellationToken.None);

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
                Email = "admin@example.com",
                DisplayName = "Admin User",
                Role = UserRole.Administrator,
                TenantId = Guid.NewGuid()
            });
        _tokenRepository.GetByUserIdAndProviderAsync(userId, "Google", Arg.Any<CancellationToken>())
            .Returns((UserIntegrationToken?)null);

        // Act
        var result = await _handler.HandleAsync(new GetGoogleConnectionStatus(), Guid.NewGuid(), CancellationToken.None);

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
        var result = await _handler.HandleAsync(new GetGoogleConnectionStatus(), Guid.NewGuid(), CancellationToken.None);

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
        var result = await _handler.HandleAsync(new GetGoogleConnectionStatus(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.IsConnected.Should().BeFalse();
        result.ConnectedAt.Should().BeNull();
    }
}
