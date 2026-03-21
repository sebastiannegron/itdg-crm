namespace Itdg.Crm.Api.Infrastructure.Options;

using System.ComponentModel.DataAnnotations;

public class GoogleOAuthOptions
{
    public const string Key = "GoogleOAuth";

    [Required]
    public required string ClientId { get; set; }

    [Required]
    public required string ClientSecret { get; set; }

    [Required]
    public required string RedirectUri { get; set; }

    public string[] Scopes { get; set; } =
    [
        "https://www.googleapis.com/auth/drive",
        "https://www.googleapis.com/auth/gmail.readonly"
    ];
}
