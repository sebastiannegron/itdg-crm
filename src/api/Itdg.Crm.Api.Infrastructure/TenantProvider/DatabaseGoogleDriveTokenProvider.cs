namespace Itdg.Crm.Api.Infrastructure.TenantProvider;

using Itdg.Crm.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// Resolves the current user's Google OAuth 2.0 access token from the database.
/// Decrypts stored tokens and refreshes expired access tokens automatically.
/// </summary>
public class DatabaseGoogleDriveTokenProvider : IGoogleDriveTokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CrmDbContext _dbContext;
    private readonly ITokenEncryptionService _encryptionService;
    private readonly IGoogleOAuthService _oAuthService;
    private readonly ILogger<DatabaseGoogleDriveTokenProvider> _logger;

    public DatabaseGoogleDriveTokenProvider(
        IHttpContextAccessor httpContextAccessor,
        CrmDbContext dbContext,
        ITokenEncryptionService encryptionService,
        IGoogleOAuthService oAuthService,
        ILogger<DatabaseGoogleDriveTokenProvider> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
        _encryptionService = encryptionService;
        _oAuthService = oAuthService;
        _logger = logger;
    }

    public string? GetAccessToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        // Look up the user by Entra object ID claim
        var entraObjectId = httpContext.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
            ?? httpContext.User.FindFirst("oid")?.Value;

        if (string.IsNullOrWhiteSpace(entraObjectId))
        {
            return null;
        }

        try
        {
            // Find user and their Google token from database
            var user = _dbContext.Set<User>()
                .IgnoreQueryFilters()
                .FirstOrDefault(u => u.EntraObjectId == entraObjectId);

            if (user is null)
            {
                return null;
            }

            var token = _dbContext.Set<UserIntegrationToken>()
                .FirstOrDefault(t => t.UserId == user.Id && t.Provider == "Google");

            if (token is null)
            {
                return null;
            }

            // Check if the access token is expired and refresh if needed
            if (token.TokenExpiry.HasValue && token.TokenExpiry.Value <= DateTimeOffset.UtcNow && token.EncryptedRefreshToken is not null)
            {
                try
                {
                    var refreshToken = _encryptionService.Decrypt(token.EncryptedRefreshToken);
                    var refreshed = _oAuthService.RefreshAccessTokenAsync(refreshToken).GetAwaiter().GetResult();

                    token.EncryptedAccessToken = _encryptionService.Encrypt(refreshed.AccessToken);
                    token.TokenExpiry = refreshed.ExpiresAt;

                    if (refreshed.RefreshToken is not null)
                    {
                        token.EncryptedRefreshToken = _encryptionService.Encrypt(refreshed.RefreshToken);
                    }

                    _dbContext.SaveChanges();

                    return refreshed.AccessToken;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to refresh Google access token for user {UserId}", user.Id);
                    return null;
                }
            }

            return _encryptionService.Decrypt(token.EncryptedAccessToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve Google access token from database");
            return null;
        }
    }
}
