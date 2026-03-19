namespace Itdg.Crm.Api.Infrastructure.Data;

using System.Linq.Expressions;
using System.Reflection;

public class CrmDbContext : DbContext, IApplicationDbContext
{
    private Guid CurrentTenantId { get; }

    private static readonly PropertyInfo CurrentTenantIdPropertyInfo =
        typeof(CrmDbContext).GetProperty(nameof(CurrentTenantId), BindingFlags.NonPublic | BindingFlags.Instance)!;

    public CrmDbContext(DbContextOptions<CrmDbContext> options, ITenantProvider tenantProvider) : base(options)
    {
        CurrentTenantId = ResolveTenantId(tenantProvider);
    }

    /// <summary>
    /// Protected constructor for derived contexts (e.g., testing).
    /// </summary>
    protected CrmDbContext(DbContextOptions options, ITenantProvider tenantProvider) : base(options)
    {
        CurrentTenantId = ResolveTenantId(tenantProvider);
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<ClientTier> ClientTiers => Set<ClientTier>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);
        ApplyGlobalQueryFilters(modelBuilder);
    }

    private static Guid ResolveTenantId(ITenantProvider tenantProvider)
    {
        try
        {
            return tenantProvider.GetTenantId();
        }
        catch (Exception ex) when (
            ex is InvalidOperationException or UnauthorizedAccessException)
        {
            // Expected during migrations or unauthenticated requests (e.g., health checks)
            // where no HTTP context or authenticated user is available.
            return Guid.Empty;
        }
    }

    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            var isSoftDeletable = typeof(ISoftDeletable).IsAssignableFrom(clrType);
            var isTenantEntity = typeof(TenantEntity).IsAssignableFrom(clrType);

            if (!isSoftDeletable && !isTenantEntity)
                continue;

            var parameter = Expression.Parameter(clrType, "e");
            Expression? filter = null;

            if (isSoftDeletable)
            {
                // e.DeletedAt == null
                var deletedAtProperty = Expression.Property(parameter, nameof(ISoftDeletable.DeletedAt));
                var nullConstant = Expression.Constant(null, typeof(DateTimeOffset?));
                filter = Expression.Equal(deletedAtProperty, nullConstant);
            }

            if (isTenantEntity)
            {
                // e.TenantId == CurrentTenantId (parameterized by EF Core per DbContext instance)
                var tenantIdProperty = Expression.Property(parameter, nameof(TenantEntity.TenantId));
                var dbContextConstant = Expression.Constant(this);
                var currentTenantIdAccess = Expression.Property(dbContextConstant, CurrentTenantIdPropertyInfo);
                var tenantFilter = Expression.Equal(tenantIdProperty, currentTenantIdAccess);

                filter = filter is not null ? Expression.AndAlso(filter, tenantFilter) : tenantFilter;
            }

            if (filter is not null)
            {
                var lambda = Expression.Lambda(filter, parameter);
                entityType.SetQueryFilter(lambda);
            }
        }
    }
}
