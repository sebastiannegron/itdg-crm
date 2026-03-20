namespace Itdg.Crm.Api.Domain.Entities;

public class ClientAssignment : TenantEntity
{
    public Guid ClientId { get; set; }
    public Client? Client { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public DateTimeOffset AssignedAt { get; set; }
}
