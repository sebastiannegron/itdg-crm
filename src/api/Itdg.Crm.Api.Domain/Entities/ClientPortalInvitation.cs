namespace Itdg.Crm.Api.Domain.Entities;

using Itdg.Crm.Api.Domain.GeneralConstants;

public class ClientPortalInvitation : TenantEntity
{
    public Guid ClientId { get; set; }
    public Client? Client { get; set; }
    public required string Email { get; set; }
    public required string Token { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
}
