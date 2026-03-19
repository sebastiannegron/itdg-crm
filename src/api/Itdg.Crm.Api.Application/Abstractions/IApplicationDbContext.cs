namespace Itdg.Crm.Api.Application.Abstractions;

public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
