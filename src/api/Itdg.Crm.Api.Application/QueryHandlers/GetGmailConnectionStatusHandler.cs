namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetGmailConnectionStatusHandler : IQueryHandler<GetGmailConnectionStatus, GmailConnectionStatusDto>
{
    private readonly IUserIntegrationTokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetGmailConnectionStatusHandler> _logger;

    public GetGmailConnectionStatusHandler(
        IUserIntegrationTokenRepository tokenRepository,
        IUserRepository userRepository,
        ICurrentUserProvider currentUserProvider,
        ILogger<GetGmailConnectionStatusHandler> logger)
    {
        _tokenRepository = tokenRepository;
        _userRepository = userRepository;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task<GmailConnectionStatusDto> HandleAsync(GetGmailConnectionStatus query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Gmail Connection Status");
        activity?.SetTag("CorrelationId", correlationId);

        var entraObjectId = _currentUserProvider.GetEntraObjectId();

        if (string.IsNullOrWhiteSpace(entraObjectId))
        {
            _logger.LogWarning("No EntraObjectId found for current user | CorrelationId: {CorrelationId}", correlationId);
            return new GmailConnectionStatusDto(IsConnected: false, ConnectedAt: null);
        }

        var user = await _userRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("User not found for EntraObjectId {EntraObjectId} | CorrelationId: {CorrelationId}", entraObjectId, correlationId);
            return new GmailConnectionStatusDto(IsConnected: false, ConnectedAt: null);
        }

        var token = await _tokenRepository.GetByUserIdAndProviderAsync(user.Id, "Gmail", cancellationToken);

        if (token is null)
        {
            return new GmailConnectionStatusDto(IsConnected: false, ConnectedAt: null);
        }

        return new GmailConnectionStatusDto(IsConnected: true, ConnectedAt: token.CreatedAt);
    }
}
