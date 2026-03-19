namespace Itdg.Crm.Api.Domain.Entities;

public class ClientTier : TenantEntity
{
    public required string Name { get; set; }
    public int SortOrder { get; set; }
}
