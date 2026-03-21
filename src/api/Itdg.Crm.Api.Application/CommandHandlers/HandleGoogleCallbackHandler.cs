namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class HandleGoogleCallbackHandler : ICommandHandler<HandleGoogleCallback>
{
    private readonly IGoogleOAuthService _oAuthService;
    private readonly ITokenEncryptionService _encryptionService;
    private readonly IUserIntegrationTokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<HandleGoogleCallbackHandler> _logger;

    public HandleGoogleCallbackHandler(
        IGoogleOAuthService oAuthService,
        ITokenEncryptionService encryptionService,
        IUserIntegrationTokenRepository tokenRepository,
        IUserRepository userRepository,
        ICurrentUserProvider currentUserProvider,
        ITenantProvider tenantProvider,
        ILogger<HandleGoogleCallbackHandler> logger)
    {
        _oAuthService = oAuthService;
        _encryptionService = encryptionService;
        _tokenRepository = tokenRepository;
        _userRepository = userRepository;
        _currentUserProvider = currentUserProvider;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task HandleAsync(HandleGoogleCallback command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Handle Google OAuth Callback");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Processing Google OAuth callback | CorrelationId: {CorrelationId}", correlationId);

        var entraObjectId = _currentUserProvider.GetEntraObjectId();

        if (string.IsNullOrWhiteSpace(entraObjectId))
        {
            _logger.LogError("No EntraObjectId found for current user | CorrelationId: {CorrelationId}", correlationId);
            throw new InvalidOperationException("User identity not available.");
        }

        var user = await _userRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);

        if (user is null)
        {
            _logger.LogError("User not found for EntraObjectId {EntraObjectId} | CorrelationId: {CorrelationId}", entraObjectId, correlationId);
            throw new InvalidOperationException("User not found.");
        }

        var tokenResponse = await _oAuthService.ExchangeCodeForTokensAsync(command.Code, cancellationToken);

        var encryptedAccessToken = _encryptionService.Encrypt(tokenResponse.AccessToken);
        var encryptedRefreshToken = tokenResponse.RefreshToken is not null
            ? _encryptionService.Encrypt(tokenResponse.RefreshToken)
            : null;

        var existingToken = await _tokenRepository.GetByUserIdAndProviderAsync(user.Id, "Google", cancellationToken);

        if (existingToken is not null)
        {
            existingToken.EncryptedAccessToken = encryptedAccessToken;
            existingToken.EncryptedRefreshToken = encryptedRefreshToken;
            existingToken.TokenExpiry = tokenResponse.ExpiresAt;
            await _tokenRepository.UpdateAsync(existingToken, cancellationToken);

            _logger.LogInformation("Updated Google tokens for user {UserId} | CorrelationId: {CorrelationId}", user.Id, correlationId);
        }
        else
        {
            var integrationToken = new UserIntegrationToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Provider = "Google",
                EncryptedAccessToken = encryptedAccessToken,
                EncryptedRefreshToken = encryptedRefreshToken,
                TokenExpiry = tokenResponse.ExpiresAt,
                TenantId = _tenantProvider.GetTenantId()
            };

            await _tokenRepository.AddAsync(integrationToken, cancellationToken);

            _logger.LogInformation("Stored Google tokens for user {UserId} | CorrelationId: {CorrelationId}", user.Id, correlationId);
        }
    }
}
