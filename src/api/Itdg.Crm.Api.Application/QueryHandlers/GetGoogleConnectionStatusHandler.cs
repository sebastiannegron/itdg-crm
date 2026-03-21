namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetGoogleConnectionStatusHandler : IQueryHandler<GetGoogleConnectionStatus, GoogleConnectionStatusDto>
{
    private readonly IUserIntegrationTokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetGoogleConnectionStatusHandler> _logger;

    public GetGoogleConnectionStatusHandler(
        IUserIntegrationTokenRepository tokenRepository,
        IUserRepository userRepository,
        ICurrentUserProvider currentUserProvider,
        ILogger<GetGoogleConnectionStatusHandler> logger)
    {
        _tokenRepository = tokenRepository;
        _userRepository = userRepository;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task<GoogleConnectionStatusDto> HandleAsync(GetGoogleConnectionStatus query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Google Connection Status");
        activity?.SetTag("CorrelationId", correlationId);

        var entraObjectId = _currentUserProvider.GetEntraObjectId();

        if (string.IsNullOrWhiteSpace(entraObjectId))
        {
            _logger.LogWarning("No EntraObjectId found for current user | CorrelationId: {CorrelationId}", correlationId);
            return new GoogleConnectionStatusDto(IsConnected: false, ConnectedAt: null);
        }

        var user = await _userRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("User not found for EntraObjectId {EntraObjectId} | CorrelationId: {CorrelationId}", entraObjectId, correlationId);
            return new GoogleConnectionStatusDto(IsConnected: false, ConnectedAt: null);
        }

        var token = await _tokenRepository.GetByUserIdAndProviderAsync(user.Id, "Google", cancellationToken);

        if (token is null)
        {
            return new GoogleConnectionStatusDto(IsConnected: false, ConnectedAt: null);
        }

        return new GoogleConnectionStatusDto(IsConnected: true, ConnectedAt: token.CreatedAt);
    }
}
