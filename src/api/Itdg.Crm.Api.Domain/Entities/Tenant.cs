namespace Itdg.Crm.Api.Domain.Entities;

public class Tenant : BaseEntity
{
    public required string Name { get; set; }
    public required string Subdomain { get; set; }
    public string? Settings { get; set; }
}
