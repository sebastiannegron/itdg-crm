namespace Itdg.Crm.Api.Application.Abstractions;

public interface IGoogleOAuthService
{
    string GetAuthorizationUrl(string state);
    Task<GoogleTokenResponse> ExchangeCodeForTokensAsync(string code, CancellationToken cancellationToken = default);
    Task<GoogleTokenResponse> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}

public record GoogleTokenResponse(
    string AccessToken,
    string? RefreshToken,
    DateTimeOffset ExpiresAt);
