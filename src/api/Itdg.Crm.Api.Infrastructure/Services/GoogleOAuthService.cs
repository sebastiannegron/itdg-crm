namespace Itdg.Crm.Api.Infrastructure.Services;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Itdg.Crm.Api.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class GoogleOAuthService : IGoogleOAuthService
{
    private readonly GoogleOAuthOptions _options;
    private readonly ILogger<GoogleOAuthService> _logger;

    public GoogleOAuthService(
        IOptions<GoogleOAuthOptions> options,
        ILogger<GoogleOAuthService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public string GetAuthorizationUrl(string state)
    {
        var flow = CreateAuthorizationCodeFlow();

        var uri = flow.CreateAuthorizationCodeRequest(_options.RedirectUri);
        uri.State = state;
        uri.ResponseType = "code";

        // Request offline access for refresh token
        var uriBuilder = new UriBuilder(uri.Build());
        var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
        query["access_type"] = "offline";
        query["prompt"] = "consent";
        uriBuilder.Query = query.ToString();

        return uriBuilder.ToString();
    }

    public async Task<GoogleTokenResponse> ExchangeCodeForTokensAsync(string code, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exchanging authorization code for tokens");

        var flow = CreateAuthorizationCodeFlow();

        var tokenResponse = await flow.ExchangeCodeForTokenAsync(
            userId: "user",
            code: code,
            redirectUri: _options.RedirectUri,
            taskCancellationToken: cancellationToken);

        var expiresAt = tokenResponse.ExpiresInSeconds.HasValue
            ? DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresInSeconds.Value)
            : DateTimeOffset.UtcNow.AddHours(1);

        return new GoogleTokenResponse(
            AccessToken: tokenResponse.AccessToken,
            RefreshToken: tokenResponse.RefreshToken,
            ExpiresAt: expiresAt);
    }

    public async Task<GoogleTokenResponse> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Refreshing Google access token");

        var flow = CreateAuthorizationCodeFlow();

        var tokenResponse = await flow.RefreshTokenAsync(
            userId: "user",
            refreshToken: refreshToken,
            taskCancellationToken: cancellationToken);

        var expiresAt = tokenResponse.ExpiresInSeconds.HasValue
            ? DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresInSeconds.Value)
            : DateTimeOffset.UtcNow.AddHours(1);

        return new GoogleTokenResponse(
            AccessToken: tokenResponse.AccessToken,
            RefreshToken: tokenResponse.RefreshToken ?? refreshToken,
            ExpiresAt: expiresAt);
    }

    private GoogleAuthorizationCodeFlow CreateAuthorizationCodeFlow()
    {
        return new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret
            },
            Scopes = _options.Scopes
        });
    }
}
