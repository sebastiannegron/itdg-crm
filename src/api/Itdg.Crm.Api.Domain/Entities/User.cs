namespace Itdg.Crm.Api.Domain.Entities;

using Itdg.Crm.Api.Domain.GeneralConstants;

public class User : TenantEntity
{
    public required string EntraObjectId { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
}
