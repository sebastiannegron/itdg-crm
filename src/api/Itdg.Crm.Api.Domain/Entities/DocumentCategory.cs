namespace Itdg.Crm.Api.Domain.Entities;

public class DocumentCategory : TenantEntity
{
    public required string Name { get; set; }
    public string? NamingConvention { get; set; }
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
}
