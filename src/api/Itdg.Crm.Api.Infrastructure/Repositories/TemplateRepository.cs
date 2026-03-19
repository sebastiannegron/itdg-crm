namespace Itdg.Crm.Api.Infrastructure.Repositories;

using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Infrastructure.Data;

public class TemplateRepository : GenericRepository<CommunicationTemplate>, ITemplateRepository
{
    public TemplateRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<CommunicationTemplate>> GetByCategoryAsync(TemplateCategory category, CancellationToken cancellationToken = default)
    {
        return await Context.Set<CommunicationTemplate>()
            .Where(t => t.Category == category)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CommunicationTemplate>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<CommunicationTemplate>()
            .Where(t => t.IsActive)
            .ToListAsync(cancellationToken);
    }
}
