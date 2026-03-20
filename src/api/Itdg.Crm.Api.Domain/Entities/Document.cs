namespace Itdg.Crm.Api.Domain.Entities;

public class Document : TenantEntity, ISoftDeletable
{
    public Guid ClientId { get; set; }
    public Guid CategoryId { get; set; }
    public DocumentCategory? Category { get; set; }
    public required string FileName { get; set; }
    public required string GoogleDriveFileId { get; set; }
    public Guid UploadedById { get; set; }
    public int CurrentVersion { get; set; }
    public long FileSize { get; set; }
    public required string MimeType { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
