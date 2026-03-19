namespace Itdg.Crm.Api.Domain.Entities;

using Itdg.Crm.Api.Domain.GeneralConstants;

public class Notification : TenantEntity, ISoftDeletable
{
    public Guid UserId { get; set; }
    public NotificationEventType EventType { get; set; }
    public NotificationChannel Channel { get; set; }
    public required string Title { get; set; }
    public required string Body { get; set; }
    public NotificationStatus Status { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
