namespace Itdg.Crm.Api.Infrastructure.Extensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Infrastructure.Data;
using Itdg.Crm.Api.Infrastructure.Interceptors;
using Itdg.Crm.Api.Infrastructure.Data.Interceptors;
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
      
        // Interceptors
        services.AddSingleton<AuditableEntityInterceptor>();
        services.AddScoped<AuditSaveChangesInterceptor>();

        // Database
        services.AddDbContext<CrmDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("CrmDb"))
              .AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
//             options.UseSqlServer(configuration.GetConnectionString(DatabaseOptions.ConnectionStringName))
//                 .AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<CrmDbContext>());

        // Tenant provider
        services.AddScoped<ITenantProvider, Itdg.Crm.Api.Infrastructure.TenantProvider.ClaimsTenantProvider>();

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // Command Handlers
        services.AddScoped<ICommandHandler<CreateTemplate>, CreateTemplateHandler>();
        services.AddScoped<ICommandHandler<UpdateTemplate>, UpdateTemplateHandler>();
        services.AddScoped<ICommandHandler<RetireTemplate>, RetireTemplateHandler>();

        // Query Handlers
        services.AddScoped<IQueryHandler<GetTemplates, IEnumerable<CommunicationTemplateDto>>, GetTemplatesHandler>();
        services.AddScoped<IQueryHandler<GetTemplateById, CommunicationTemplateDto>, GetTemplateByIdHandler>();

        return services;
    }
}
