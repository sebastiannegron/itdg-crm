namespace Itdg.Crm.Api.Infrastructure.Repositories;

using Itdg.Crm.Api.Infrastructure.Data;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly CrmDbContext Context;

    public GenericRepository(CrmDbContext context)
    {
        Context = context;
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<T>().FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<T>().ToListAsync(cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await Context.Set<T>().AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        Context.Set<T>().Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        Context.Set<T>().Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }
}
