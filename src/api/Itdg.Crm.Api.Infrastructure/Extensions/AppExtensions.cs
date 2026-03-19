namespace Itdg.Crm.Api.Infrastructure.Extensions;

using Itdg.Crm.Api.Infrastructure.Data;
using Itdg.Crm.Api.Infrastructure.Options;
using Itdg.Crm.Api.Infrastructure.Repositories;

public static class AppExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<CrmDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("CrmDb")));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<CrmDbContext>());

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        return services;
    }
}
