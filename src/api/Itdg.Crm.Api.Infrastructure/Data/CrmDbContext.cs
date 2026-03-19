namespace Itdg.Crm.Api.Infrastructure.Data;

using System.Linq.Expressions;
using System.Reflection;

public class CrmDbContext : DbContext, IApplicationDbContext
{
    private readonly Guid _currentTenantId;

    private static readonly FieldInfo CurrentTenantIdField =
        typeof(CrmDbContext).GetField(nameof(_currentTenantId), BindingFlags.NonPublic | BindingFlags.Instance)!;

    public CrmDbContext(DbContextOptions<CrmDbContext> options, ITenantProvider tenantProvider) : base(options)
    {
        _currentTenantId = InitializeTenantId(tenantProvider);
    }

    /// <summary>
    /// Protected constructor for derived contexts (e.g., testing).
    /// </summary>
    protected CrmDbContext(DbContextOptions options, ITenantProvider tenantProvider) : base(options)
    {
        _currentTenantId = InitializeTenantId(tenantProvider);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ApplyGlobalQueryFilters(modelBuilder);
    }

    private static Guid InitializeTenantId(ITenantProvider tenantProvider)
    {
        try
        {
            return tenantProvider.GetTenantId();
        }
        catch
        {
            // During migrations or unauthenticated requests (e.g., health checks),
            // the tenant ID is not available. Default to Guid.Empty.
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
                // e.TenantId == _currentTenantId (parameterized by EF Core per DbContext instance)
                var tenantIdProperty = Expression.Property(parameter, nameof(TenantEntity.TenantId));
                var dbContextConstant = Expression.Constant(this);
                var currentTenantIdAccess = Expression.Field(dbContextConstant, CurrentTenantIdField);
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
