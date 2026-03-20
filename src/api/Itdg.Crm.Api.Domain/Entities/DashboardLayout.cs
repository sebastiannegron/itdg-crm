namespace Itdg.Crm.Api.Domain.Entities;

public class DashboardLayout : TenantEntity
{
    public Guid UserId { get; set; }
    public string? WidgetConfigurations { get; set; }
}
