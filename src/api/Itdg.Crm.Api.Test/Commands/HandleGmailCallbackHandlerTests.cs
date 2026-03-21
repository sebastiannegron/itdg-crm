namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class HandleGmailCallbackHandlerTests
{
    private readonly IGoogleOAuthService _oAuthService;
    private readonly ITokenEncryptionService _encryptionService;
    private readonly IUserIntegrationTokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<HandleGmailCallbackHandler> _logger;
    private readonly HandleGmailCallbackHandler _handler;

    public HandleGmailCallbackHandlerTests()
    {
        _oAuthService = Substitute.For<IGoogleOAuthService>();
        _encryptionService = Substitute.For<ITokenEncryptionService>();
        _tokenRepository = Substitute.For<IUserIntegrationTokenRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _currentUserProvider = Substitute.For<ICurrentUserProvider>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _logger = Substitute.For<ILogger<HandleGmailCallbackHandler>>();
        _handler = new HandleGmailCallbackHandler(
            _oAuthService, _encryptionService, _tokenRepository, _userRepository,
            _currentUserProvider, _tenantProvider, _logger);
    }

    [Fact]
    public async Task HandleAsync_CreatesNewToken_WhenNoExistingToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var code = "auth-code-123";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        _currentUserProvider.GetEntraObjectId().Returns("entra-123");
        _userRepository.GetByEntraObjectIdAsync("entra-123", Arg.Any<CancellationToken>())
            .Returns(new User
            {
                Id = userId,
                EntraObjectId = "entra-123",
                Email = "user@example.com",
                DisplayName = "Test User",
                Role = UserRole.Associate,
                TenantId = tenantId
            });
        _oAuthService.ExchangeCodeForTokensAsync(code, Arg.Any<CancellationToken>())
            .Returns(new GoogleTokenResponse("access-token", "refresh-token", expiresAt));
        _encryptionService.Encrypt("access-token").Returns("encrypted-access");
        _encryptionService.Encrypt("refresh-token").Returns("encrypted-refresh");
        _tenantProvider.GetTenantId().Returns(tenantId);
        _tokenRepository.GetByUserIdAndProviderAsync(userId, "Gmail", Arg.Any<CancellationToken>())
            .Returns((UserIntegrationToken?)null);

        UserIntegrationToken? capturedToken = null;
        _tokenRepository.AddAsync(Arg.Do<UserIntegrationToken>(t => capturedToken = t), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(new HandleGmailCallback(code), "en", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _tokenRepository.Received(1).AddAsync(Arg.Any<UserIntegrationToken>(), Arg.Any<CancellationToken>());
        capturedToken.Should().NotBeNull();
        capturedToken!.UserId.Should().Be(userId);
        capturedToken.Provider.Should().Be("Gmail");
        capturedToken.EncryptedAccessToken.Should().Be("encrypted-access");
        capturedToken.EncryptedRefreshToken.Should().Be("encrypted-refresh");
        capturedToken.TokenExpiry.Should().Be(expiresAt);
        capturedToken.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task HandleAsync_UpdatesExistingToken_WhenTokenAlreadyExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "auth-code-456";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

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
        _oAuthService.ExchangeCodeForTokensAsync(code, Arg.Any<CancellationToken>())
            .Returns(new GoogleTokenResponse("new-access-token", "new-refresh-token", expiresAt));
        _encryptionService.Encrypt("new-access-token").Returns("encrypted-new-access");
        _encryptionService.Encrypt("new-refresh-token").Returns("encrypted-new-refresh");

        var existingToken = new UserIntegrationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = "Gmail",
            EncryptedAccessToken = "old-encrypted-access",
            EncryptedRefreshToken = "old-encrypted-refresh",
            TokenExpiry = DateTimeOffset.UtcNow.AddHours(-1),
            TenantId = Guid.NewGuid()
        };
        _tokenRepository.GetByUserIdAndProviderAsync(userId, "Gmail", Arg.Any<CancellationToken>())
            .Returns(existingToken);

        // Act
        await _handler.HandleAsync(new HandleGmailCallback(code), "en", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _tokenRepository.Received(1).UpdateAsync(existingToken, Arg.Any<CancellationToken>());
        await _tokenRepository.DidNotReceive().AddAsync(Arg.Any<UserIntegrationToken>(), Arg.Any<CancellationToken>());
        existingToken.EncryptedAccessToken.Should().Be("encrypted-new-access");
        existingToken.EncryptedRefreshToken.Should().Be("encrypted-new-refresh");
        existingToken.TokenExpiry.Should().Be(expiresAt);
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenUserNotFound()
    {
        // Arrange
        _currentUserProvider.GetEntraObjectId().Returns("entra-999");
        _userRepository.GetByEntraObjectIdAsync("entra-999", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act & Assert
        await FluentActions.Invoking(() =>
            _handler.HandleAsync(new HandleGmailCallback("code"), "en", Guid.NewGuid(), CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenEntraObjectIdIsNull()
    {
        // Arrange
        _currentUserProvider.GetEntraObjectId().Returns((string?)null);

        // Act & Assert
        await FluentActions.Invoking(() =>
            _handler.HandleAsync(new HandleGmailCallback("code"), "en", Guid.NewGuid(), CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User identity not available.");
    }

    [Fact]
    public async Task HandleAsync_HandlesNullRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "auth-code-no-refresh";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

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
        _oAuthService.ExchangeCodeForTokensAsync(code, Arg.Any<CancellationToken>())
            .Returns(new GoogleTokenResponse("access-token", null, expiresAt));
        _encryptionService.Encrypt("access-token").Returns("encrypted-access");
        _tenantProvider.GetTenantId().Returns(Guid.NewGuid());
        _tokenRepository.GetByUserIdAndProviderAsync(userId, "Gmail", Arg.Any<CancellationToken>())
            .Returns((UserIntegrationToken?)null);

        UserIntegrationToken? capturedToken = null;
        _tokenRepository.AddAsync(Arg.Do<UserIntegrationToken>(t => capturedToken = t), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(new HandleGmailCallback(code), "en", Guid.NewGuid(), CancellationToken.None);

        // Assert
        capturedToken.Should().NotBeNull();
        capturedToken!.EncryptedRefreshToken.Should().BeNull();
    }
}
