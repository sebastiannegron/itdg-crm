namespace Itdg.Crm.Api.Domain.Entities;

using Itdg.Crm.Api.Domain.GeneralConstants;

public class NotificationPreference : TenantEntity
{
    public Guid UserId { get; set; }
    public NotificationEventType EventType { get; set; }
    public NotificationChannel Channel { get; set; }
    public bool IsEnabled { get; set; }
    public required string DigestMode { get; set; }
}
