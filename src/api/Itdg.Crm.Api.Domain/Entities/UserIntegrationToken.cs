namespace Itdg.Crm.Api.Domain.Entities;

public class UserIntegrationToken : TenantEntity
{
    public Guid UserId { get; set; }

    [Required]
    [StringLength(50)]
    public required string Provider { get; set; }

    [Required]
    public required string EncryptedAccessToken { get; set; }

    public string? EncryptedRefreshToken { get; set; }

    public DateTimeOffset? TokenExpiry { get; set; }

    // Navigation property
    public User? User { get; set; }
}
