namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class DisconnectGoogleHandlerTests
{
    private readonly IUserIntegrationTokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<DisconnectGoogleHandler> _logger;
    private readonly DisconnectGoogleHandler _handler;

    public DisconnectGoogleHandlerTests()
    {
        _tokenRepository = Substitute.For<IUserIntegrationTokenRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _currentUserProvider = Substitute.For<ICurrentUserProvider>();
        _logger = Substitute.For<ILogger<DisconnectGoogleHandler>>();
        _handler = new DisconnectGoogleHandler(_tokenRepository, _userRepository, _currentUserProvider, _logger);
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
            Provider = "Google",
            EncryptedAccessToken = "encrypted",
            TenantId = Guid.NewGuid()
        };

        _currentUserProvider.GetEntraObjectId().Returns("entra-123");
        _userRepository.GetByEntraObjectIdAsync("entra-123", Arg.Any<CancellationToken>())
            .Returns(new User
            {
                Id = userId,
                EntraObjectId = "entra-123",
                Email = "admin@example.com",
                DisplayName = "Admin",
                Role = UserRole.Administrator,
                TenantId = Guid.NewGuid()
            });
        _tokenRepository.GetByUserIdAndProviderAsync(userId, "Google", Arg.Any<CancellationToken>())
            .Returns(token);

        // Act
        await _handler.HandleAsync(new DisconnectGoogle(), "en", Guid.NewGuid(), CancellationToken.None);

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
                Email = "admin@example.com",
                DisplayName = "Admin",
                Role = UserRole.Administrator,
                TenantId = Guid.NewGuid()
            });
        _tokenRepository.GetByUserIdAndProviderAsync(userId, "Google", Arg.Any<CancellationToken>())
            .Returns((UserIntegrationToken?)null);

        // Act
        await _handler.HandleAsync(new DisconnectGoogle(), "en", Guid.NewGuid(), CancellationToken.None);

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
        await _handler.HandleAsync(new DisconnectGoogle(), "en", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _tokenRepository.DidNotReceive().DeleteAsync(Arg.Any<UserIntegrationToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DoesNothing_WhenEntraObjectIdIsNull()
    {
        // Arrange
        _currentUserProvider.GetEntraObjectId().Returns((string?)null);

        // Act
        await _handler.HandleAsync(new DisconnectGoogle(), "en", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _tokenRepository.DidNotReceive().DeleteAsync(Arg.Any<UserIntegrationToken>(), Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().GetByEntraObjectIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
