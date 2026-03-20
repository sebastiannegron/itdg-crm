namespace Itdg.Crm.Api.Infrastructure.Repositories;

using Itdg.Crm.Api.Infrastructure.Data;

public class MessageRepository : GenericRepository<Message>, IMessageRepository
{
    public MessageRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Message>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Message>()
            .Where(m => m.ClientId == clientId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Message?> GetByIdAndClientIdAsync(Guid id, Guid clientId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Message>()
            .FirstOrDefaultAsync(m => m.Id == id && m.ClientId == clientId, cancellationToken);
    }
}
