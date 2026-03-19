namespace Itdg.Crm.Api.Infrastructure.Repositories;

using Itdg.Crm.Api.Infrastructure.Data;

public class ClientRepository : GenericRepository<Client>, IClientRepository
{
    public ClientRepository(CrmDbContext context) : base(context)
    {
    }
}
