namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class DisconnectGmailHandlerTests
{
    private readonly IUserIntegrationTokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<DisconnectGmailHandler> _logger;
    private readonly DisconnectGmailHandler _handler;

    public DisconnectGmailHandlerTests()
    {
        _tokenRepository = Substitute.For<IUserIntegrationTokenRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _currentUserProvider = Substitute.For<ICurrentUserProvider>();
        _logger = Substitute.For<ILogger<DisconnectGmailHandler>>();
        _handler = new DisconnectGmailHandler(_tokenRepository, _userRepository, _currentUserProvider, _logger);
    }

    [Fact]
    public async Task HandleAsync_DeletesToken_WhenTokenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = new UserIntegrationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = "Gmail",
            EncryptedAccessToken = "encrypted",
            TenantId = Guid.NewGuid()
        };

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
            .Returns(token);

        // Act
        await _handler.HandleAsync(new DisconnectGmail(), "en", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _tokenRepository.Received(1).DeleteAsync(token, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DoesNothing_WhenNoTokenExists()
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
        await _handler.HandleAsync(new DisconnectGmail(), "en", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _tokenRepository.DidNotReceive().DeleteAsync(Arg.Any<UserIntegrationToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DoesNothing_WhenUserNotFound()
    {
        // Arrange
        _currentUserProvider.GetEntraObjectId().Returns("entra-999");
        _userRepository.GetByEntraObjectIdAsync("entra-999", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        await _handler.HandleAsync(new DisconnectGmail(), "en", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _tokenRepository.DidNotReceive().DeleteAsync(Arg.Any<UserIntegrationToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DoesNothing_WhenEntraObjectIdIsNull()
    {
        // Arrange
        _currentUserProvider.GetEntraObjectId().Returns((string?)null);

        // Act
        await _handler.HandleAsync(new DisconnectGmail(), "en", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _tokenRepository.DidNotReceive().DeleteAsync(Arg.Any<UserIntegrationToken>(), Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().GetByEntraObjectIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
