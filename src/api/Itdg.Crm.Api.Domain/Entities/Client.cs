namespace Itdg.Crm.Api.Domain.Entities;

using Itdg.Crm.Api.Domain.GeneralConstants;

public class Client : TenantEntity, ISoftDeletable
{
    public required string Name { get; set; }
    public string? ContactEmail { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public Guid? TierId { get; set; }
    public ClientTier? Tier { get; set; }
    public ClientStatus Status { get; set; }
    public string? IndustryTag { get; set; }
    public string? Notes { get; set; }
    public string? CustomFields { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
