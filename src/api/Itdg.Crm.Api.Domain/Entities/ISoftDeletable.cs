namespace Itdg.Crm.Api.Domain.Entities;

public interface ISoftDeletable
{
    DateTimeOffset? DeletedAt { get; set; }
}
