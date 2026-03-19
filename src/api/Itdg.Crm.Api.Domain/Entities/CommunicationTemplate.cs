namespace Itdg.Crm.Api.Domain.Entities;

using Itdg.Crm.Api.Domain.Enums;

public class CommunicationTemplate : TenantEntity, ISoftDeletable
{
    public TemplateCategory Category { get; set; }
    public required string Name { get; set; }
    public required string SubjectTemplate { get; set; }
    public required string BodyTemplate { get; set; }
    public required string Language { get; set; }
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public Guid CreatedById { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
