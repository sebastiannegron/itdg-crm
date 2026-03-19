namespace Itdg.Crm.Api.Infrastructure.Extensions;

using Itdg.Crm.Api.Infrastructure.Data;
using Itdg.Crm.Api.Infrastructure.Data.Interceptors;
using Itdg.Crm.Api.Infrastructure.Options;
using Itdg.Crm.Api.Infrastructure.Repositories;

public static class AppExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // HttpContext accessor for claims resolution
        services.AddHttpContextAccessor();

        // Interceptors
        services.AddSingleton<AuditableEntityInterceptor>();

        // Database
        services.AddDbContext<CrmDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString(DatabaseOptions.ConnectionStringName))
                .AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<CrmDbContext>());

        // Tenant provider
        services.AddScoped<ITenantProvider, Itdg.Crm.Api.Infrastructure.TenantProvider.ClaimsTenantProvider>();

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        return services;
    }
}
