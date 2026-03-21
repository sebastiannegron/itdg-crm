namespace Itdg.Crm.Api.Domain.Entities;

public class EmailMirror : TenantEntity
{
    public Guid ClientId { get; set; }

    public required string GmailMessageId { get; set; }

    public required string GmailThreadId { get; set; }

    public required string Subject { get; set; }

    public required string From { get; set; }

    public required string To { get; set; }

    public string? BodyPreview { get; set; }

    public bool HasAttachments { get; set; }

    public DateTimeOffset ReceivedAt { get; set; }
}
