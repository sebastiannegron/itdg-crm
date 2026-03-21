namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Microsoft.Extensions.Logging;

public class GetGoogleAuthUrlHandler : IQueryHandler<GetGoogleAuthUrl, string>
{
    private readonly IGoogleOAuthService _oAuthService;
    private readonly ILogger<GetGoogleAuthUrlHandler> _logger;

    public GetGoogleAuthUrlHandler(
        IGoogleOAuthService oAuthService,
        ILogger<GetGoogleAuthUrlHandler> logger)
    {
        _oAuthService = oAuthService;
        _logger = logger;
    }

    public Task<string> HandleAsync(GetGoogleAuthUrl query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Google Auth URL");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Generating Google OAuth authorization URL | CorrelationId: {CorrelationId}", correlationId);

        var state = correlationId.ToString();
        var url = _oAuthService.GetAuthorizationUrl(state);

        return Task.FromResult(url);
    }
}
