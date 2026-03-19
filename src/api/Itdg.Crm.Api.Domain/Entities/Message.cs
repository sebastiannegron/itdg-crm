namespace Itdg.Crm.Api.Domain.Entities;

using Itdg.Crm.Api.Domain.Enums;

public class Message : TenantEntity, ISoftDeletable
{
    public Guid ClientId { get; set; }
    public Guid SenderId { get; set; }
    public MessageDirection Direction { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public Guid? TemplateId { get; set; }
    public bool IsPortalMessage { get; set; }
    public bool IsRead { get; set; }
    public string? Attachments { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
