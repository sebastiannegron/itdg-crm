namespace Itdg.Crm.Api.Infrastructure.Extensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Itdg.Crm.Api.Infrastructure.Data;
using Itdg.Crm.Api.Infrastructure.Options;
using Itdg.Crm.Api.Infrastructure.Repositories;

public static class AppExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // HttpContext accessor for claims resolution
        services.AddHttpContextAccessor();

        // Authentication — Microsoft Entra ID (JWT Bearer)
        services.AddMicrosoftIdentityWebApiAuthentication(configuration, AzureAdOptions.Key);

        // Authorization — require authenticated user by default
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        // AzureAd options validation
        services.AddOptionsWithValidateOnStart<AzureAdOptions>()
            .Bind(configuration.GetSection(AzureAdOptions.Key))
            .ValidateDataAnnotations();

        // Database
        services.AddDbContext<CrmDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("CrmDb")));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<CrmDbContext>());

        // Tenant provider
        services.AddScoped<ITenantProvider, Itdg.Crm.Api.Infrastructure.TenantProvider.ClaimsTenantProvider>();

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        return services;
    }
}
