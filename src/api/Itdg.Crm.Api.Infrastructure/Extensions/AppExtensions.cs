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
using Itdg.Crm.Api.Infrastructure.Services;
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

        services.AddScoped<DbContext>(provider =>
            provider.GetRequiredService<CrmDbContext>());

        // Tenant provider
        services.AddScoped<ITenantProvider, TenantProvider.ClaimsTenantProvider>();

        // Current user provider
        services.AddScoped<ICurrentUserProvider, TenantProvider.ClaimsCurrentUserProvider>();

        // Google Drive token provider
        services.AddScoped<IGoogleDriveTokenProvider, TenantProvider.ClaimsGoogleDriveTokenProvider>();

        // Gmail options validation
        services.AddOptionsWithValidateOnStart<GmailOptions>()
            .Bind(configuration.GetSection(GmailOptions.Key))
            .ValidateDataAnnotations();

        // Google Drive options validation
        services.AddOptionsWithValidateOnStart<GoogleDriveOptions>()
            .Bind(configuration.GetSection(GoogleDriveOptions.Key))
            .ValidateDataAnnotations();

        // Gmail sync options validation
        services.AddOptionsWithValidateOnStart<GmailSyncOptions>()
            .Bind(configuration.GetSection(GmailSyncOptions.Key))
            .ValidateDataAnnotations();

        // Document purge options validation
        services.AddOptionsWithValidateOnStart<DocumentPurgeOptions>()
            .Bind(configuration.GetSection(DocumentPurgeOptions.Key))
            .ValidateDataAnnotations();

        // Portal options validation
        services.AddOptionsWithValidateOnStart<PortalOptions>()
            .Bind(configuration.GetSection(PortalOptions.Key))
            .ValidateDataAnnotations();

        // Azure OpenAI options validation
        services.AddOptionsWithValidateOnStart<AzureOpenAiOptions>()
            .Bind(configuration.GetSection(AzureOpenAiOptions.Key))
            .ValidateDataAnnotations();

        // SignalR
        services.AddSignalR();

        // Services
        services.AddSingleton<ITemplateRenderer, TemplateRenderer>();
        services.AddScoped<IEmailSender, NoOpEmailSender>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IGmailService, GmailService>();
        services.AddScoped<IGoogleDriveService, GoogleDriveService>();
        services.AddScoped<IPortalConfiguration, PortalConfiguration>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAiDraftingService, AzureOpenAiDraftingService>();

        // Background services
        services.AddHostedService<GmailSyncBackgroundService>();
        services.AddHostedService<DocumentPurgeBackgroundService>();

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IClientAssignmentRepository, ClientAssignmentRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IDashboardLayoutRepository, DashboardLayoutRepository>();
        services.AddScoped<IEmailMirrorRepository, EmailMirrorRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Command handlers
        services.AddScoped<ICommandHandler<CreateClient>, CreateClientHandler>();
        services.AddScoped<ICommandHandler<UpdateClient>, UpdateClientHandler>();
        services.AddScoped<ICommandHandler<DeleteClient>, DeleteClientHandler>();
        services.AddScoped<ICommandHandler<CreateTemplate>, CreateTemplateHandler>();
        services.AddScoped<ICommandHandler<UpdateTemplate>, UpdateTemplateHandler>();
        services.AddScoped<ICommandHandler<RetireTemplate>, RetireTemplateHandler>();
        services.AddScoped<ICommandHandler<SendPortalMessage>, SendPortalMessageHandler>();
        services.AddScoped<ICommandHandler<MarkMessageAsRead>, MarkMessageAsReadHandler>();
        services.AddScoped<ICommandHandler<SendTemplateMessage>, SendTemplateMessageHandler>();
        services.AddScoped<ICommandHandler<AssignClient>, AssignClientHandler>();
        services.AddScoped<ICommandHandler<UnassignClient>, UnassignClientHandler>();
        services.AddScoped<ICommandHandler<UpdateUser>, UpdateUserHandler>();
        services.AddScoped<ICommandHandler<InviteUser>, InviteUserHandler>();
        services.AddScoped<ICommandHandler<CreateTier>, CreateTierHandler>();
        services.AddScoped<ICommandHandler<UpdateTier>, UpdateTierHandler>();
        services.AddScoped<ICommandHandler<SaveDashboardLayout>, SaveDashboardLayoutHandler>();
        services.AddScoped<ICommandHandler<CreateDocumentCategory>, CreateDocumentCategoryHandler>();
        services.AddScoped<ICommandHandler<UpdateDocumentCategory>, UpdateDocumentCategoryHandler>();
        services.AddScoped<ICommandHandler<DeleteDocumentCategory>, DeleteDocumentCategoryHandler>();
        services.AddScoped<ICommandHandler<ReorderDocumentCategories>, ReorderDocumentCategoriesHandler>();
        services.AddScoped<ICommandHandler<MarkNotificationAsRead>, MarkNotificationAsReadHandler>();
        services.AddScoped<ICommandHandler<MarkAllNotificationsAsRead>, MarkAllNotificationsAsReadHandler>();
        services.AddScoped<ICommandHandler<UpdateNotificationPreferences>, UpdateNotificationPreferencesHandler>();
        services.AddScoped<ICommandHandler<UploadDocument>, UploadDocumentHandler>();
        services.AddScoped<ICommandHandler<DeleteDocument>, DeleteDocumentHandler>();
        services.AddScoped<ICommandHandler<RestoreDocument>, RestoreDocumentHandler>();
        services.AddScoped<ICommandHandler<UploadNewVersion>, UploadNewVersionHandler>();
        services.AddScoped<ICommandHandler<InviteClient>, InviteClientHandler>();
        services.AddScoped<ICommandHandler<UploadPortalDocument>, UploadPortalDocumentHandler>();

        // Query handlers
        services.AddScoped<IQueryHandler<GetClientById, ClientDto>, GetClientByIdHandler>();
        services.AddScoped<IQueryHandler<GetClients, PaginatedResultDto<ClientDto>>, GetClientsHandler>();
        services.AddScoped<IQueryHandler<GetTemplates, IEnumerable<CommunicationTemplateDto>>, GetTemplatesHandler>();
        services.AddScoped<IQueryHandler<GetTemplateById, CommunicationTemplateDto>, GetTemplateByIdHandler>();
        services.AddScoped<IQueryHandler<GetPortalMessages, IEnumerable<MessageDto>>, GetPortalMessagesHandler>();
        services.AddScoped<IQueryHandler<GetPortalMessageById, MessageDto>, GetPortalMessageByIdHandler>();
        services.AddScoped<IQueryHandler<RenderTemplate, RenderedTemplateDto>, RenderTemplateHandler>();
        services.AddScoped<IQueryHandler<GetUsers, PaginatedResultDto<UserDto>>, GetUsersHandler>();
        services.AddScoped<IQueryHandler<GetUserById, UserDto>, GetUserByIdHandler>();
        services.AddScoped<IQueryHandler<GetTiers, IEnumerable<ClientTierDto>>, GetTiersHandler>();
        services.AddScoped<IQueryHandler<GetClientAssignments, IEnumerable<ClientAssignmentDto>>, GetClientAssignmentsHandler>();
        services.AddScoped<IQueryHandler<GetDashboardSummary, DashboardSummaryDto>, GetDashboardSummaryHandler>();
        services.AddScoped<IQueryHandler<GetDashboardLayout, DashboardLayoutDto?>, GetDashboardLayoutHandler>();
        services.AddScoped<IQueryHandler<GetDocumentCategories, IEnumerable<DocumentCategoryDto>>, GetDocumentCategoriesHandler>();
        services.AddScoped<IQueryHandler<GetClientDocuments, PaginatedResultDto<DocumentDto>>, GetClientDocumentsHandler>();
        services.AddScoped<IQueryHandler<DownloadDocument, DocumentDownloadDto>, DownloadDocumentHandler>();
        services.AddScoped<IQueryHandler<GetRecycleBin, PaginatedResultDto<RecycleBinDocumentDto>>, GetRecycleBinHandler>();
        services.AddScoped<IQueryHandler<GetDocumentDetail, DocumentDetailDto>, GetDocumentDetailHandler>();
        services.AddScoped<IQueryHandler<GetNotifications, PaginatedResultDto<NotificationDto>>, GetNotificationsHandler>();
        services.AddScoped<IQueryHandler<GetUnreadNotificationCount, int>, GetUnreadNotificationCountHandler>();
        services.AddScoped<IQueryHandler<GetNotificationPreferences, IEnumerable<NotificationPreferenceDto>>, GetNotificationPreferencesHandler>();
        services.AddScoped<IQueryHandler<GetPortalDocuments, PaginatedResultDto<DocumentDto>>, GetPortalDocumentsHandler>();
        services.AddScoped<IQueryHandler<GetClientTimeline, PaginatedResultDto<TimelineItemDto>>, GetClientTimelineHandler>();
        services.AddScoped<IQueryHandler<GetClientEmails, PaginatedResultDto<EmailMirrorDto>>, GetClientEmailsHandler>();
        services.AddScoped<IQueryHandler<GetDocumentAuditTrail, PaginatedResultDto<AuditLogDto>>, GetDocumentAuditTrailHandler>();
        services.AddScoped<IQueryHandler<DraftEmail, string>, DraftEmailHandler>();

        return services;
    }
}
