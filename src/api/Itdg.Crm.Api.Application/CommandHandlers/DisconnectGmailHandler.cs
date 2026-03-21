namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class DisconnectGmailHandler : ICommandHandler<DisconnectGmail>
{
    private readonly IUserIntegrationTokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<DisconnectGmailHandler> _logger;

    public DisconnectGmailHandler(
        IUserIntegrationTokenRepository tokenRepository,
        IUserRepository userRepository,
        ICurrentUserProvider currentUserProvider,
        ILogger<DisconnectGmailHandler> logger)
    {
        _tokenRepository = tokenRepository;
        _userRepository = userRepository;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task HandleAsync(DisconnectGmail command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Disconnect Gmail Account");
        activity?.SetTag("CorrelationId", correlationId);

        var entraObjectId = _currentUserProvider.GetEntraObjectId();

        if (string.IsNullOrWhiteSpace(entraObjectId))
        {
            _logger.LogWarning("No EntraObjectId found for current user | CorrelationId: {CorrelationId}", correlationId);
            return;
        }

        var user = await _userRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("User not found for EntraObjectId {EntraObjectId} | CorrelationId: {CorrelationId}", entraObjectId, correlationId);
            return;
        }

        var token = await _tokenRepository.GetByUserIdAndProviderAsync(user.Id, "Gmail", cancellationToken);

        if (token is null)
        {
            _logger.LogInformation("No Gmail connection found for user {UserId} | CorrelationId: {CorrelationId}", user.Id, correlationId);
            return;
        }

        await _tokenRepository.DeleteAsync(token, cancellationToken);

        _logger.LogInformation("Disconnected Gmail account for user {UserId} | CorrelationId: {CorrelationId}", user.Id, correlationId);
    }
}
