namespace Itdg.Crm.Api.Infrastructure.Repositories;

using Itdg.Crm.Api.Infrastructure.Data;

public class EmailMirrorRepository : GenericRepository<EmailMirror>, IEmailMirrorRepository
{
    public EmailMirrorRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<EmailMirror>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<EmailMirror>()
            .Where(e => e.ClientId == clientId)
            .OrderByDescending(e => e.ReceivedAt)
            .ToListAsync(cancellationToken);
    }
}
