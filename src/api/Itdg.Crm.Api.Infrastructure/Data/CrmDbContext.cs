namespace Itdg.Crm.Api.Infrastructure.Data;

public class CrmDbContext : DbContext, IApplicationDbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
