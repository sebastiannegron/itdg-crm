namespace Itdg.Crm.Api.Infrastructure.Options;

using System.ComponentModel.DataAnnotations;

public class TokenEncryptionOptions
{
    public const string Key = "TokenEncryption";

    [Required]
    [StringLength(44, MinimumLength = 44)]
    public required string EncryptionKey { get; set; }
}
