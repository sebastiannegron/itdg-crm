namespace Itdg.Crm.Api.Domain.Repositories;

using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Enums;

public interface ITemplateRepository : IGenericRepository<CommunicationTemplate>
{
    Task<IReadOnlyList<CommunicationTemplate>> GetByCategoryAsync(TemplateCategory category, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommunicationTemplate>> GetActiveAsync(CancellationToken cancellationToken = default);
}
