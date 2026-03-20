namespace Itdg.Crm.Api.Domain.Entities;

public class DocumentVersion : BaseEntity
{
    public Guid DocumentId { get; set; }
    public Document? Document { get; set; }
    public int VersionNumber { get; set; }
    public required string GoogleDriveFileId { get; set; }
    public Guid UploadedById { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
}
