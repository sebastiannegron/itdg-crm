namespace Itdg.Crm.Api.Infrastructure.Extensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Infrastructure.Authorization;
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

        // Authorization — require authenticated user by default + role-based policies
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build())
            .AddPolicy(AuthorizationPolicyNames.Administrator, policy =>
                policy.RequireRole(nameof(UserRole.Administrator)))
            .AddPolicy(AuthorizationPolicyNames.Associate, policy =>
                policy.RequireRole(nameof(UserRole.Administrator), nameof(UserRole.Associate)))
            .AddPolicy(AuthorizationPolicyNames.ClientPortal, policy =>
                policy.RequireRole(nameof(UserRole.ClientPortal)))
            .AddPolicy(AuthorizationPolicyNames.ClientAssignment, policy =>
                policy.AddRequirements(new ClientAssignmentRequirement()));

        // Authorization handler
        services.AddScoped<IAuthorizationHandler, ClientAssignmentAuthorizationHandler>();

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
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IClientAssignmentRepository, ClientAssignmentRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        // Command handlers
        services.AddScoped<ICommandHandler<CreateClient>, CreateClientHandler>();
        services.AddScoped<ICommandHandler<UpdateClient>, UpdateClientHandler>();
        services.AddScoped<ICommandHandler<DeleteClient>, DeleteClientHandler>();
        services.AddScoped<ICommandHandler<CreateTemplate>, CreateTemplateHandler>();
        services.AddScoped<ICommandHandler<UpdateTemplate>, UpdateTemplateHandler>();
        services.AddScoped<ICommandHandler<RetireTemplate>, RetireTemplateHandler>();
        services.AddScoped<ICommandHandler<SendPortalMessage>, SendPortalMessageHandler>();
        services.AddScoped<ICommandHandler<MarkMessageAsRead>, MarkMessageAsReadHandler>();
        services.AddScoped<ICommandHandler<AssignClient>, AssignClientHandler>();
        services.AddScoped<ICommandHandler<UnassignClient>, UnassignClientHandler>();
      
        // Query handlers
        services.AddScoped<IQueryHandler<GetClientById, ClientDto>, GetClientByIdHandler>();
        services.AddScoped<IQueryHandler<GetClients, PaginatedResultDto<ClientDto>>, GetClientsHandler>();
        services.AddScoped<IQueryHandler<GetTemplates, IEnumerable<CommunicationTemplateDto>>, GetTemplatesHandler>();
        services.AddScoped<IQueryHandler<GetTemplateById, CommunicationTemplateDto>, GetTemplateByIdHandler>();
        services.AddScoped<IQueryHandler<GetPortalMessages, IEnumerable<MessageDto>>, GetPortalMessagesHandler>();
        services.AddScoped<IQueryHandler<GetPortalMessageById, MessageDto>, GetPortalMessageByIdHandler>();

        return services;
    }
}
