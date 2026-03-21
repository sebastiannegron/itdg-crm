namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Microsoft.Extensions.Logging;

public class GetGmailAuthUrlHandler : IQueryHandler<GetGmailAuthUrl, string>
{
    private readonly IGoogleOAuthService _oAuthService;
    private readonly ILogger<GetGmailAuthUrlHandler> _logger;

    public GetGmailAuthUrlHandler(
        IGoogleOAuthService oAuthService,
        ILogger<GetGmailAuthUrlHandler> logger)
    {
        _oAuthService = oAuthService;
        _logger = logger;
    }

    public Task<string> HandleAsync(GetGmailAuthUrl query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Gmail Auth URL");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Generating Gmail OAuth authorization URL | CorrelationId: {CorrelationId}", correlationId);

        var state = correlationId.ToString();
        var url = _oAuthService.GetAuthorizationUrl(state);

        return Task.FromResult(url);
    }
}
