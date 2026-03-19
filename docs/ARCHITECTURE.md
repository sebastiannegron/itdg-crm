# ITDG CRM Platform — Architecture

> **Version:** 2.0
> **Last Updated:** 2026-03-18
> **Status:** Draft — pending team review
> **Source:** [ITDG - CRM Requirements v2.docx](./ITDG%20-%20CRM%20Requirements%20v2.docx)

---

## 1. System Overview

A web-based CRM platform for a tax consulting practice in Puerto Rico. The system centralizes client management, document collection, task workflows, communications, and payment collection — integrating with the firm's existing Google Workspace ecosystem (Gmail, Calendar, Drive).

**Key constraints:**
- Multi-tenant from day one (single tenant initially)
- ~150 concurrent users during peak tax season
- PCI DSS v4.0 compliance for payment data (Phase 2)
- All timestamps in `America/Puerto_Rico` timezone
- English admin UI at launch; Spanish template support from day one; full bilingual UI in Phase 4

---

## 2. High-Level System Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         CLIENTS (Browser)                           │
│                    Admin UI  │  Associate UI  │  Client Portal      │
└──────────────────────────────┼───────────────────────────────────────┘
                               │ HTTPS (TLS 1.3)
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│                     Azure App Service (Frontend)                     │
│                     Next.js 16 — App Router                         │
│          Server Components │ Server Actions │ API Routes            │
└──────────────────────────────┼───────────────────────────────────────┘
                               │ HTTPS
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│                     Azure App Service (Backend)                      │
│                   ASP.NET Core Minimal API (.NET 9)                 │
│                                                                     │
│  ┌──────────┐  ┌───────────┐  ┌────────────┐  ┌────────────────┐  │
│  │   API    │  │Application│  │   Domain   │  │Infrastructure  │  │
│  │(Minimal  │→ │ (Custom   │→ │ (Entities, │← │ (EF Core, API  │  │
│  │Endpoints)│  │  CQRS)    │  │  Repos)    │  │  Clients, etc) │  │
│  └──────────┘  └───────────┘  └────────────┘  └────────────────┘  │
│                                                                     │
│  ┌──────────────┐                                                  │
│  │ Diagnostics  │  (OpenTelemetry ActivitySource)                   │
│  └──────────────┘                                                  │
└───────┬──────────┬──────────────┬──────────────┬────────────────────┘
        │          │              │              │
        ▼          ▼              ▼              ▼
┌──────────┐ ┌──────────┐ ┌───────────┐ ┌──────────────────────────┐
│Azure SQL │ │Azure Key │ │ Microsoft │ │   External Services      │
│Database  │ │  Vault   │ │ Entra ID  │ │                          │
│(SQL Svr) │ │          │ │           │ │ • Gmail API              │
└──────────┘ └──────────┘ └───────────┘ │ • Google Calendar API    │
                                        │ • Google Drive API       │
┌──────────────────────────┐            │ • Azure OpenAI Service   │
│     Azure Services       │            │ • Microsoft Graph API    │
│ • Application Insights   │            │ • Payment Partner (Ph2)  │
│ • Azure AI Search        │            │ • QuickBooks Online (Ph4)│
│ • Azure Blob Storage     │            └──────────────────────────┘
│   (temp upload staging)  │
└──────────────────────────┘
```

---

## 3. Tech Stack

| Layer | Technology | Notes |
|---|---|---|
| **Frontend** | Next.js 16 / React 19 / TypeScript 5 | App Router, Server Components, ESM |
| **UI Components** | shadcn/ui + Tailwind CSS 4 | Design system with custom CRM theme |
| **Forms** | react-hook-form + Zod 4 | 4-file page convention |
| **Internationalization** | next-intl 4 | `[locale]` routing with `en-pr` / `es-pr` |
| **Observability (FE)** | OpenTelemetry + Azure Monitor | Spans in all server actions and API routes |
| **Backend** | .NET 9 / ASP.NET Core Minimal API / C# 13 | Clean Architecture |
| **API Style** | Minimal APIs | `MapGroup` + static endpoint classes (no MVC controllers) |
| **CQRS** | Custom ITDG implementation | `ICommand`/`IQuery<T>` + handlers (no MediatR) |
| **ORM** | Entity Framework Core 9 | Code-first migrations |
| **Validation** | FluentValidation (explicit per-endpoint) | No auto-pipeline — called in endpoint body |
| **Auth** | Microsoft Entra ID (OpenID Connect) | MSAL.js (frontend) + Microsoft.Identity.Web (backend) |
| **Database** | Azure SQL Database (SQL Server) | Encrypted at rest (AES-256) |
| **Search** | Azure AI Search | Full-text document search |
| **Email Delivery** | Microsoft Graph API | Notifications, templates, campaigns (via shared mailbox or service account) |
| **AI** | Azure OpenAI Service | Email drafting assistance |
| **Observability (BE)** | OpenTelemetry + Azure Monitor / Application Insights | ActivitySource in Diagnostics project |
| **JSON** | System.Text.Json | `snake_case` via `[JsonPropertyName]` |
| **CI/CD** | GitHub Actions | Build, test, deploy to Azure |
| **IaC** | Azure Bicep | All infrastructure as code |
| **Testing** | xUnit + FluentAssertions + NSubstitute (API) / Vitest + RTL (Frontend) | |

---

## 4. Repository Structure

Monorepo with clearly separated frontend and backend projects:

```
itdg-crm/
├── .github/
│   ├── copilot-instructions.md     # Copilot coding agent instructions
│   ├── ISSUE_TEMPLATE/
│   │   ├── task.yml
│   │   └── bug.yml
│   └── workflows/
│       ├── api-ci.yml              # Backend CI pipeline
│       ├── web-ci.yml              # Frontend CI pipeline
│       └── deploy.yml              # Deployment pipeline
│
├── docs/
│   ├── ARCHITECTURE.md             # This file
│   ├── EPICS.md                    # Epic/story/task breakdown
│   ├── api/                        # API documentation (OpenAPI specs)
│   └── adr/                        # Architecture Decision Records
│
├── src/
│   ├── api/                               # ── .NET Backend ──
│   │   ├── Itdg.Crm.sln
│   │   ├── Directory.Build.props
│   │   ├── global.json
│   │   │
│   │   ├── Itdg.Crm.Api/                 # API Host (composition root)
│   │   │   ├── Endpoints/                 # Minimal API route groups (static classes)
│   │   │   ├── HostedServices/            # BackgroundService workers
│   │   │   ├── Middlewares/               # Custom HTTP middleware
│   │   │   ├── Options/                   # Configuration POCO classes
│   │   │   ├── Requests/                  # Request models + FluentValidation validators
│   │   │   ├── Extensions/                # OpenAPI filters, etc.
│   │   │   ├── Program.cs
│   │   │   ├── appsettings.json
│   │   │   └── GlobalUsings.cs
│   │   │
│   │   ├── Itdg.Crm.Api.Application/     # Business Logic (CQRS)
│   │   │   ├── Abstractions/              # ICommandHandler, IQueryHandler, ICacheService
│   │   │   ├── Commands/                  # Command records (implement ICommand)
│   │   │   ├── CommandHandlers/           # Command handler implementations
│   │   │   ├── Queries/                   # Query records (implement IQuery<TResult>)
│   │   │   ├── QueryHandlers/             # Query handler implementations
│   │   │   ├── Dtos/                      # Response DTOs (record types, snake_case JSON)
│   │   │   ├── Exceptions/                # Domain exceptions with ErrorCode property
│   │   │   ├── Helpers/                   # Utility helpers
│   │   │   └── CustomAttributes/          # Custom validation attributes
│   │   │
│   │   ├── Itdg.Crm.Api.Infrastructure/  # Data Access & External Services
│   │   │   ├── Data/                      # EF Core DbContext, IApplicationDbContext
│   │   │   ├── Extensions/                # AppExtensions.cs (AddInfrastructure — single entry point)
│   │   │   ├── Options/                   # Infrastructure options
│   │   │   ├── Repositories/              # GenericRepository<T> + specializations
│   │   │   └── Services/                  # CacheService, Google clients, Microsoft Graph, etc.
│   │   │
│   │   ├── Itdg.Crm.Api.Domain/          # Zero NuGet dependencies
│   │   │   ├── Entities/                  # EF Core entities
│   │   │   ├── Repositories/              # IGenericRepository<T> + domain interfaces
│   │   │   └── GeneralConstants/          # Enums and constants
│   │   │
│   │   ├── Itdg.Crm.Api.Diagnostics/     # OpenTelemetry ActivitySource
│   │   │   └── DiagnosticsConfig.cs
│   │   │
│   │   └── Itdg.Crm.Api.Test/            # xUnit tests (single project)
│   │       ├── Commands/
│   │       ├── Queries/
│   │       ├── Endpoints/
│   │       └── Repositories/
│   │
│   └── web/                               # ── Next.js Frontend ──
│       ├── package.json
│       ├── next.config.ts
│       ├── tailwind.config.ts
│       ├── tsconfig.json
│       ├── instrumentation.ts             # OTel entry point
│       ├── instrumentation.node.ts        # OTel Node SDK config
│       │
│       ├── app/
│       │   ├── [locale]/
│       │   │   ├── _shared/
│       │   │   │   ├── app-fieldnames.ts  # Typed i18n dictionary (en-pr, es-pr)
│       │   │   │   └── app-enums.ts       # PageStatus, regex constants, enums
│       │   │   │
│       │   │   ├── (admin)/               # Admin/associate layout group
│       │   │   │   ├── dashboard/
│       │   │   │   │   ├── page.tsx           # Server component (data fetch)
│       │   │   │   │   ├── DashboardView.tsx  # Client component
│       │   │   │   │   ├── shared.ts          # Zod schemas + types
│       │   │   │   │   └── actions.ts         # Server actions
│       │   │   │   ├── clients/
│       │   │   │   │   ├── page.tsx
│       │   │   │   │   ├── ClientsView.tsx
│       │   │   │   │   ├── shared.ts
│       │   │   │   │   └── actions.ts
│       │   │   │   │   └── [client_id]/       # snake_case dynamic params
│       │   │   │   │       ├── page.tsx
│       │   │   │   │       ├── ClientDetailView.tsx
│       │   │   │   │       ├── shared.ts
│       │   │   │   │       └── actions.ts
│       │   │   │   ├── documents/
│       │   │   │   ├── communications/
│       │   │   │   ├── tasks/
│       │   │   │   ├── settings/
│       │   │   │   └── layout.tsx
│       │   │   │
│       │   │   ├── (portal)/              # Client portal layout group
│       │   │   │   ├── portal/
│       │   │   │   │   ├── messages/
│       │   │   │   │   ├── documents/
│       │   │   │   │   └── payments/
│       │   │   │   └── layout.tsx
│       │   │   │
│       │   │   ├── layout.tsx
│       │   │   └── page.tsx
│       │   │
│       │   ├── _components/               # Shared components
│       │   │   ├── ui/                    # shadcn/ui primitives
│       │   │   ├── DataTable.tsx
│       │   │   ├── PageHeader.tsx
│       │   │   ├── EmptyState.tsx
│       │   │   ├── LoadingSpinner.tsx
│       │   │   └── ErrorBoundary.tsx
│       │   │
│       │   ├── api/                       # Next.js API routes
│       │   │   └── {resource}/{action}/route.ts
│       │   └── auth/
│       │
│       ├── server/
│       │   └── Services/
│       │       ├── api-client.ts          # Base fetch client for .NET API
│       │       ├── clientService.ts
│       │       ├── documentService.ts
│       │       ├── communicationService.ts
│       │       ├── notificationService.ts
│       │       └── dashboardService.ts
│       │
│       ├── hooks/                         # Custom React hooks
│       │   ├── use-notifications.ts
│       │   └── use-auth.ts
│       │
│       ├── i18n/
│       │   ├── routing.ts
│       │   └── request.ts
│       │
│       └── tests/
│           ├── components/
│           └── e2e/
│
├── infra/                                 # Azure Bicep IaC
│   ├── main.bicep
│   ├── modules/
│   │   ├── app-service.bicep
│   │   ├── sql-database.bicep
│   │   ├── key-vault.bicep
│   │   └── monitoring.bicep
│   └── parameters/
│       ├── dev.bicepparam
│       └── prod.bicepparam
│
├── .editorconfig
├── .gitignore
├── CLAUDE.md                              # Claude Code project instructions
└── README.md
```

---

## 5. Backend Architecture (Clean Architecture + Minimal APIs)

### Layer Dependencies

```
Itdg.Crm.Api → Itdg.Crm.Api.Application → Itdg.Crm.Api.Domain
Itdg.Crm.Api → Itdg.Crm.Api.Infrastructure → Itdg.Crm.Api.Application → Itdg.Crm.Api.Domain
Itdg.Crm.Api → Itdg.Crm.Api.Diagnostics
Itdg.Crm.Api.Application → Itdg.Crm.Api.Diagnostics
Itdg.Crm.Api.Test → Itdg.Crm.Api.Infrastructure
```

- **Domain** has **zero NuGet dependencies**. Contains entities, repository interfaces, enums, and constants.
- **Application** depends only on Domain + Diagnostics. Contains custom CQRS abstractions, command/query handlers, DTOs, and domain exceptions.
- **Infrastructure** implements interfaces from Domain and Application. Contains EF Core (`Data/`), repository implementations, and external service clients.
- **API** is the composition root. Minimal API endpoints, hosted services, middleware, request models, and DI registration.
- **Diagnostics** defines the `ActivitySource` for OpenTelemetry. Referenced by API and Application.

### Custom CQRS Pattern (No MediatR)

ITDG uses a custom CQRS implementation — **do NOT use MediatR**.

**Abstractions** (in Application):

```csharp
public interface ICommand { }
public interface IQuery<TResult> { }

public interface ICommandHandler<in TCommand> where TCommand : class, ICommand
{
    Task HandleAsync(TCommand command, string language, Guid correlationId, CancellationToken cancellationToken);
}

public interface IQueryHandler<in TQuery, TResult> where TQuery : class, IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, Guid correlationId, CancellationToken cancellationToken);
}
```

**Command and Query records:**

```csharp
// File: Itdg.Crm.Api.Application/Commands/CreateClient.cs
public record CreateClient(Guid ClientId, string Name, string Email, int TierId) : ICommand;

// File: Itdg.Crm.Api.Application/Queries/GetClientById.cs
public record GetClientById(Guid ClientId) : IQuery<ClientDto?>;
```

**Handler implementation:**

```csharp
// File: Itdg.Crm.Api.Application/CommandHandlers/CreateClientHandler.cs
public class CreateClientHandler : ICommandHandler<CreateClient>
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<CreateClientHandler> _logger;

    public CreateClientHandler(IClientRepository clientRepository, ILogger<CreateClientHandler> logger)
    {
        _clientRepository = clientRepository;
        _logger = logger;
    }

    public async Task HandleAsync(CreateClient command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Create Client");
        activity?.SetTag("CorrelationId", correlationId);

        // business logic...
    }
}
```

### Minimal API Endpoints

Each domain area is a **static class** with a `Map{Area}Endpoints` extension returning `RouteGroupBuilder`:

```csharp
// File: Itdg.Crm.Api/Endpoints/ClientsEndpoints.cs
public static class ClientsEndpoints
{
    public static RouteGroupBuilder MapClientsEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("/api/v1/Clients");
        group.WithTags("Clients");

        group.MapGet("", GetClientsEndpoint)
            .RequireAuthorization(policy => policy.RequireRole("Clients.Read", "Clients.ReadWrite"))
            .WithName("GetClients")
            .Produces<IEnumerable<ClientDto>>(StatusCodes.Status200OK);

        group.MapPost("", CreateClientEndpoint)
            .RequireAuthorization(policy => policy.RequireRole("Clients.Write", "Clients.ReadWrite"))
            .WithName("CreateClient")
            .Produces<ClientCreatedDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        return group;
    }

    private static async Task<IResult> CreateClientEndpoint(
        HttpContext httpContext,
        IValidator<CreateClientRequest> validator,
        ICommandHandler<CreateClient> handler,
        ILogger<ClientsEndpoints> logger,
        CreateClientRequest request,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];

        try
        {
            ValidationResult result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
            {
                logger.LogWarning("Validation failed | CorrelationId: {CorrelationId}", correlationId);
                return Results.ValidationProblem(result.ToDictionary());
            }

            var command = new CreateClient(Guid.NewGuid(), request.Name, request.Email, request.TierId);
            await handler.HandleAsync(command, "en-pr", Guid.Parse(correlationId!), cancellationToken);

            return Results.Created($"/api/v1/Clients/{command.ClientId}",
                new ClientCreatedDto { ClientId = command.ClientId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create client | CorrelationId: {CorrelationId}", correlationId);
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Server Error",
                extensions: new Dictionary<string, object?> { { "errorCode", "create_client_failed" } });
        }
    }
}
```

**Registration in `Program.cs`:**

```csharp
app.MapClientsEndpoints();
app.MapDocumentsEndpoints();
app.MapCommunicationsEndpoints();
app.MapTemplatesEndpoints();
app.MapNotificationsEndpoints();
app.MapDashboardEndpoints();
app.MapPortalEndpoints();
```

### Request Models and Validation

Request models live in `Itdg.Crm.Api/Requests/` — separate from commands/queries:

```csharp
// File: Itdg.Crm.Api/Requests/CreateClientRequest.cs
public class CreateClientRequest
{
    [JsonPropertyName("name")]
    [Required, StringLength(200, MinimumLength = 2)]
    public required string Name { get; set; }

    [JsonPropertyName("email")]
    [Required, EmailAddress]
    public required string Email { get; set; }

    [JsonPropertyName("tier_id")]
    [Required]
    public required int TierId { get; set; }
}

// File: Itdg.Crm.Api/Requests/CreateClientRequestValidator.cs
public class CreateClientRequestValidator : AbstractValidator<CreateClientRequest>
{
    public CreateClientRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.TierId).GreaterThan(0);
    }
}
```

### Error Handling

**Per-endpoint try-catch** with correlation ID logging. Return `ProblemDetails` with an `errorCode` extension. **No global exception middleware.**

Domain exceptions carry an `ErrorCode` property:

```csharp
// File: Itdg.Crm.Api.Application/Exceptions/ClientNotFoundException.cs
public class ClientNotFoundException : Exception
{
    public Guid ClientId { get; }
    public string ErrorCode => "client_not_found";
    public ClientNotFoundException(Guid clientId) : base($"Client with id {clientId} was not found.") => ClientId = clientId;
}
```

### Dependency Injection

All DI is centralized in a **single** `AddInfrastructure` extension in the Infrastructure layer:

```csharp
// File: Itdg.Crm.Api.Infrastructure/Extensions/AppExtensions.cs
public static class AppExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<IApplicationDbContext, CrmDbContext>(options =>
            options.UseSqlServer(configuration.GetRequiredConnectionString("CrmDb")));

        // Caching
        services.AddMemoryCache();
        services.AddScoped<ICacheService, CacheService>();

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        // Command Handlers
        services.AddScoped<ICommandHandler<CreateClient>, CreateClientHandler>();
        services.AddScoped<ICommandHandler<UpdateClient>, UpdateClientHandler>();

        // Query Handlers
        services.AddScoped<IQueryHandler<GetClientById, ClientDto?>, GetClientByIdHandler>();
        services.AddScoped<IQueryHandler<GetClients, IEnumerable<ClientDto>>, GetClientsHandler>();

        // External Services
        services.AddScoped<IGoogleDriveService, GoogleDriveService>();
        services.AddScoped<IGmailService, GmailService>();
        services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();
        services.AddScoped<IEmailSender, MicrosoftGraphEmailSender>();
        services.AddScoped<IAiDraftingService, AzureOpenAiDraftingService>();

        return services;
    }
}
```

Called from `Program.cs`: `builder.Services.AddInfrastructure(builder.Configuration);`

### Options Pattern

Every configuration section maps to a POCO with `const string Key` and `[Required]` annotations:

```csharp
// File: Itdg.Crm.Api/Options/GoogleWorkspaceOptions.cs
public class GoogleWorkspaceOptions
{
    public const string Key = "GoogleWorkspace";

    [Required] public required string ClientId { get; set; }
    [Required] public required string ClientSecret { get; set; }
    [Required] public required string[] Scopes { get; set; }
}
```

Registration: `services.AddOptionsWithValidateOnStart<GoogleWorkspaceOptions>().Bind(configuration.GetSection(GoogleWorkspaceOptions.Key)).ValidateDataAnnotations();`

### JSON Serialization

- Use `System.Text.Json` — **not** Newtonsoft
- All JSON properties use **`snake_case`** via `[JsonPropertyName]`
- DTOs use `record` types (immutable); request models use `class` with `required`

```csharp
public record ClientDto(
    [property: JsonPropertyName("client_id")] Guid ClientId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("tier_name")] string TierName,
    [property: JsonPropertyName("status")] string Status
);
```

### Multi-Tenancy

- Tenant ID stored on every entity via a base `TenantEntity` class
- EF Core global query filter ensures data isolation: `.HasQueryFilter(e => e.TenantId == currentTenantId)`
- Tenant resolved from the authenticated user's JWT claims via `ITenantProvider`
- Tenant context passed to handlers via the correlation pipeline

### Authentication & Authorization

- **Microsoft Entra ID** via OpenID Connect
- Frontend uses **MSAL.js** to acquire tokens; backend validates JWT with `Microsoft.Identity.Web`
- Three built-in roles: `Administrator`, `Associate`, `ClientPortal`
- Role-based per endpoint: `.RequireAuthorization(policy => policy.RequireRole("Clients.ReadWrite"))`
- Client-level data segregation enforced via `ClientAssignment` — associates only see assigned clients

### Observability

- **Diagnostics project** defines a shared `ActivitySource`:

```csharp
// File: Itdg.Crm.Api.Diagnostics/DiagnosticsConfig.cs
public static class DiagnosticsConfig
{
    public const string ServiceName = "Itdg.Crm.Api";
    public static readonly ActivitySource ActivitySource = new(ServiceName);
}
```

- Every handler wraps logic in `DiagnosticsConfig.ActivitySource.StartActivity("Operation Name")`
- Logs via `ILogger<T>` with structured properties and correlation IDs
- Exported to Azure Monitor / Application Insights via OpenTelemetry

---

## 6. Database Schema

### Entity Relationship Diagram (Core)

```
┌──────────┐       ┌──────────────────┐       ┌────────────┐
│  Tenant  │──1:N──│      User        │──N:M──│   Client   │
└──────────┘       │ (EntraId, Role)  │       │ (Tier,     │
                   └──────────────────┘       │  Status)   │
                           │                  └─────┬──────┘
                    ClientAssignment                │
                                              ┌─────┴──────┐
                               ┌──────────────┼────────────┐│
                               ▼              ▼            ▼│
                        ┌──────────┐  ┌────────────┐  ┌────┴──────┐
                        │ Document │  │   Message   │  │   Task    │
                        │(Category,│  │(Template,   │  │(Status,   │
                        │ Version) │  │ Direction)  │  │ Priority) │
                        └──────────┘  └────────────┘  └───────────┘
                                            │
                                      ┌─────┴──────┐
                                      ▼            ▼
                               ┌──────────┐ ┌───────────┐
                               │ Template │ │EmailMirror│
                               │(Category,│ │(Gmail     │
                               │ Version) │ │ thread)   │
                               └──────────┘ └───────────┘
```

### Core Entities

| Entity | Key Fields | Notes |
|---|---|---|
| `Tenant` | Id, Name, Subdomain, Settings | Multi-tenant root |
| `User` | Id, TenantId, EntraObjectId, Email, Role, IsActive | Maps to Entra ID |
| `Client` | Id, TenantId, Name, ContactEmail, Phone, Address, TierId, Status, IndustryTag | Core client record |
| `ClientAssignment` | ClientId, UserId, AssignedAt | N:M — which associates see which clients |
| `ClientTier` | Id, TenantId, Name, SortOrder | Tier 1/2/3 (admin-configurable labels) |
| `Document` | Id, TenantId, ClientId, CategoryId, FileName, GoogleDriveFileId, UploadedById, Version | Points to Google Drive |
| `DocumentVersion` | Id, DocumentId, VersionNumber, GoogleDriveFileId, UploadedById, UploadedAt | Version history |
| `DocumentCategory` | Id, TenantId, Name, NamingConvention | Bank Statements, Tax Forms, etc. |
| `Message` | Id, TenantId, ClientId, SenderId, Direction (Inbound/Outbound), Subject, Body, TemplateId?, IsPortalMessage | Client communications |
| `CommunicationTemplate` | Id, TenantId, Category, Name, SubjectTemplate, BodyTemplate, Language, Version, IsActive | Merge-field templates |
| `EmailMirror` | Id, TenantId, ClientId, GmailMessageId, GmailThreadId, Subject, From, To, ReceivedAt | Mirrored Gmail messages |
| `Notification` | Id, TenantId, UserId, EventType, Channel, Title, Body, Status, DeliveredAt, ReadAt | Unified notification engine |
| `NotificationPreference` | Id, UserId, EventType, Channel, IsEnabled, DigestMode | Per-user preferences |
| `Task` | Id, TenantId, ClientId, Title, Description, AssignedToId, DueDate, Priority, Status, WorkflowTemplateStepId? | Phase 2 |
| `WorkflowTemplate` | Id, TenantId, Name, Description | Phase 2: repeatable task sequences |
| `PaymentMethod` | Id, TenantId, ClientId, Type (Card/ACH), TokenizedRef, Last4, ExpiresAt, ConsentId | Phase 2 — PCI tokenized |
| `PaymentTransaction` | Id, TenantId, ClientId, PaymentMethodId, Amount, Status, ReferenceNumber, InitiatedById | Phase 2 |
| `PaymentConsent` | Id, TenantId, ClientId, ConsentVersion, AuthorizedMethods, RecordedById, Timestamp | Phase 2 — digital consent |
| `AuditLog` | Id, TenantId, UserId, EntityType, EntityId, Action, OldValues, NewValues, Timestamp | All data changes |

### Indexes (Performance-Critical)

- `IX_Client_TenantId_Status` — client list filtering
- `IX_Document_TenantId_ClientId_CategoryId` — document browsing
- `IX_EmailMirror_TenantId_ClientId_ReceivedAt` — email timeline
- `IX_Notification_UserId_Status` — notification center
- `IX_Task_TenantId_AssignedToId_Status` — task board queries
- `IX_AuditLog_TenantId_EntityType_EntityId` — audit lookup

---

## 7. API Design

### Convention

- **Minimal API** route groups with consistent naming: `/api/v1/{Resource}`
- Error responses use `ProblemDetails` with `errorCode` extension
- Pagination via `?page=1&pageSize=25`
- API versioning via URL path (`/api/v1/`, `/api/v2/`)
- OpenAPI/Swagger documentation auto-generated
- Correlation ID passed via `X-Correlation-Id` header

### Core Endpoints (MVP)

```
Authentication (handled by Entra ID — no custom endpoints)

Clients — MapClientsEndpoints()
  GET    /api/v1/Clients                    # List (filtered by assignment)
  GET    /api/v1/Clients/{client_id}        # Detail
  POST   /api/v1/Clients                    # Create (admin only)
  PUT    /api/v1/Clients/{client_id}        # Update
  GET    /api/v1/Clients/{client_id}/Timeline      # Unified activity timeline
  POST   /api/v1/Clients/{client_id}/Assignments   # Assign associate

Documents — MapDocumentsEndpoints()
  GET    /api/v1/Clients/{client_id}/Documents     # List by client
  POST   /api/v1/Clients/{client_id}/Documents     # Upload
  GET    /api/v1/Documents/{document_id}            # Download
  GET    /api/v1/Documents/{document_id}/Versions   # Version history
  DELETE /api/v1/Documents/{document_id}             # Soft delete → recycle bin
  POST   /api/v1/Documents/Search                    # Full-text search

Communications — MapCommunicationsEndpoints()
  GET    /api/v1/Clients/{client_id}/Messages       # Portal messages
  POST   /api/v1/Clients/{client_id}/Messages       # Send message
  GET    /api/v1/Clients/{client_id}/Emails          # Mirrored Gmail
  POST   /api/v1/Clients/{client_id}/Emails/Sync     # Trigger email sync

Templates — MapTemplatesEndpoints()
  GET    /api/v1/Templates                            # List templates
  POST   /api/v1/Templates                            # Create (admin)
  PUT    /api/v1/Templates/{template_id}              # Update (admin)
  POST   /api/v1/Templates/{template_id}/Render       # Render with merge fields

AI Drafting — MapAiEndpoints()
  POST   /api/v1/Ai/DraftEmail                        # Generate draft

Notifications — MapNotificationsEndpoints()
  GET    /api/v1/Notifications                        # Current user's notifications
  PUT    /api/v1/Notifications/{notification_id}/Read  # Mark as read
  GET    /api/v1/Notifications/Preferences            # Get preferences
  PUT    /api/v1/Notifications/Preferences            # Update preferences

Dashboard — MapDashboardEndpoints()
  GET    /api/v1/Dashboard/Summary                    # Aggregated dashboard data
  GET    /api/v1/Dashboard/Calendar                   # Google Calendar events

Portal — MapPortalEndpoints()
  GET    /api/v1/Portal/Messages                      # My messages
  POST   /api/v1/Portal/Messages                      # Reply
  GET    /api/v1/Portal/Documents                     # My documents
  POST   /api/v1/Portal/Documents                     # Upload document
  GET    /api/v1/Portal/Payments                      # My payment history (Phase 2)
```

### Phase 2 Additions

```
Tasks — MapTasksEndpoints()
  GET    /api/v1/Tasks                                # List (Kanban/list view)
  POST   /api/v1/Tasks                                # Create
  PUT    /api/v1/Tasks/{task_id}                      # Update (status, assignee)
  GET    /api/v1/Clients/{client_id}/Tasks            # Tasks by client

Payments — MapPaymentsEndpoints()
  GET    /api/v1/Clients/{client_id}/PaymentMethods   # List stored methods
  POST   /api/v1/Clients/{client_id}/PaymentMethods   # Add method (admin)
  POST   /api/v1/Clients/{client_id}/Payments         # Initiate payment
  GET    /api/v1/Clients/{client_id}/Payments          # Transaction history
  GET    /api/v1/Dashboard/Payments                    # Payment summary widget
```

---

## 8. Frontend Architecture

### Routing Strategy

All pages are nested under `[locale]/` for internationalization:

```
app/[locale]/(admin)/...     → Admin and associate UI (Entra ID login, role-based nav)
app/[locale]/(portal)/...    → Client portal (separate branded layout, invitation-based)
```

Locales: `en-pr` (English — Puerto Rico) and `es-pr` (Spanish — Puerto Rico).

**Routing setup:**

```typescript
// i18n/routing.ts
export const routing = defineRouting({
  locales: ["en-pr", "es-pr"],
  defaultLocale: "en-pr",
  localePrefix: "always",
  localeDetection: false,
});
export const { Link, redirect, usePathname, useRouter } = createNavigation(routing);
```

Always import `Link`, `redirect`, `usePathname`, `useRouter` from `@/i18n/routing` — **never** from `next/navigation` or `next/link`.

### 4-File Page Convention

Every page follows exactly these 4 files:

| File | Role | Directive |
|---|---|---|
| `page.tsx` | Server Component — fetches data, passes to form/view | (none — server by default) |
| `{Name}Form.tsx` or `{Name}View.tsx` | Client Component — interactive UI | `"use client"` |
| `shared.ts` | Zod schemas (locale-aware), DTO types, constants | (none) |
| `actions.ts` | Server Actions — validate, call service, wrapped in OTel spans | `"use server"` |

### Data Fetching

- **Server Components** fetch data directly via `server/Services/` and pass to client components as props
- **Server Actions** handle form submissions — validate with Zod, call service functions, return `{ success, errors }`
- **Next.js API routes** (`app/api/`) for webhooks and external callbacks only
- **No TanStack Query** — use server components + server actions for data flow
- Service functions in `server/Services/` call the .NET API backend via fetch

**Service function pattern:**

```typescript
// server/Services/api-client.ts
const API_BASE = process.env.API_BASE_URL!;

export async function apiFetch<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers: { "Content-Type": "application/json", ...options?.headers },
  });
  if (!res.ok) throw new Error(`API error: ${res.status}`);
  return res.json();
}

// server/Services/clientService.ts
import { apiFetch } from "./api-client";

export async function getClients(): Promise<ClientDto[]> {
  return apiFetch<ClientDto[]>("/api/v1/Clients");
}
```

### Internationalization

User-facing text lives in `app-fieldnames.ts`, not JSON files:

```typescript
// app/[locale]/_shared/app-fieldnames.ts
export const fieldnames = {
  "en-pr": {
    clients_title: "Clients",
    name_label: "Name",
    required_error: "This field is required",
    email_invalid_error: "Invalid email address",
  },
  "es-pr": {
    clients_title: "Clientes",
    name_label: "Nombre",
    required_error: "Este campo es requerido",
    email_invalid_error: "Correo electr\u00f3nico inv\u00e1lido",
  },
} as const;
export type Locale = keyof typeof fieldnames;
```

### Form State Management

All forms use a standard `PageStatus` type:

```typescript
// app/[locale]/_shared/app-enums.ts
export type PageStatus = "idle" | "loading" | "success" | "failed";
```

### Input Security

All user text inputs must be validated against injection patterns via Zod `.refine()`:

```typescript
// app/[locale]/_shared/app-enums.ts
export const urlRegex = /(https?:\/\/[^\s]+)|(www\.[^\s]+)|([a-zA-Z0-9-]+\.(com|net|org|edu|gov|io|biz|info|me))/i;
export const codeRegex = /<[^>]*>|{.*}|\[.*\]|function\s*\(|eval\(|=>/i;
```

Applied in Zod schemas:

```typescript
z.string().min(1).refine(val => !codeRegex.test(val) && !urlRegex.test(val), "Invalid input");
```

### OpenTelemetry

Every server action and API route is wrapped in an OTel span:

```typescript
// actions.ts
"use server";
import { Span, SpanStatusCode, trace } from "@opentelemetry/api";

export async function createClient(formData: FormData) {
  return trace.getTracer("web").startActiveSpan("Create Client", async (span: Span) => {
    try {
      // validate, call service, return result
      span.setStatus({ code: SpanStatusCode.OK });
      return { success: true, message: "" };
    } catch (err: any) {
      span.recordException(err);
      span.setStatus({ code: SpanStatusCode.ERROR, message: `${err}` });
      return { success: false, message: "" };
    } finally {
      span.end();
    }
  });
}
```

### Component Organization

- **`_components/`** — shared components (shadcn/ui in `_components/ui/`, plus `DataTable`, `PageHeader`, etc.)
- **Page-specific components** live in their page folder (the `{Name}Form.tsx` / `{Name}View.tsx` file)
- No barrel exports (`index.ts`) — import components directly
- Prefix internal directories with `_`

---

## 9. UI/UX Design System

### Design Principles

- **Professional and trustworthy** — financial/tax industry aesthetic
- **Data-dense but clean** — CRM screens show a lot of information; clarity is critical
- **Mobile-first responsive** — usable on phones during client meetings and in the field
- **Accessible** — WCAG 2.1 AA compliance; sufficient contrast, keyboard navigation, screen reader support
- **Two distinct experiences** — admin UI (feature-rich, sidebar nav) and client portal (simple, branded)

### Color Palette

Built on shadcn/ui CSS variables in `tailwind.config.ts` and `globals.css`:

| Token | Light Mode | Dark Mode | Usage |
|---|---|---|---|
| `--primary` | `hsl(220, 70%, 45%)` | `hsl(220, 70%, 60%)` | Buttons, active nav, links — professional blue |
| `--primary-foreground` | `hsl(0, 0%, 100%)` | `hsl(0, 0%, 100%)` | Text on primary backgrounds |
| `--secondary` | `hsl(215, 20%, 95%)` | `hsl(215, 20%, 18%)` | Subtle backgrounds, secondary buttons |
| `--accent` | `hsl(160, 60%, 40%)` | `hsl(160, 60%, 50%)` | Success states, positive metrics, completed tasks |
| `--destructive` | `hsl(0, 72%, 51%)` | `hsl(0, 72%, 61%)` | Errors, failed payments, delete actions |
| `--warning` | `hsl(38, 92%, 50%)` | `hsl(38, 92%, 60%)` | Deadline approaching, pending items, alerts |
| `--muted` | `hsl(215, 16%, 93%)` | `hsl(215, 16%, 16%)` | Disabled elements, placeholder text |
| `--background` | `hsl(0, 0%, 100%)` | `hsl(220, 15%, 10%)` | Page background |
| `--card` | `hsl(0, 0%, 100%)` | `hsl(220, 15%, 13%)` | Card and widget surfaces |
| `--border` | `hsl(215, 20%, 88%)` | `hsl(215, 20%, 22%)` | Borders, dividers |
| `--ring` | `hsl(220, 70%, 45%)` | `hsl(220, 70%, 60%)` | Focus rings |

**Tier colors** (for client classification badges):

| Tier | Color | Token |
|---|---|---|
| Tier 1 (High) | Gold | `hsl(45, 93%, 47%)` |
| Tier 2 (Medium) | Blue | `hsl(220, 70%, 55%)` |
| Tier 3 (Standard) | Slate | `hsl(215, 16%, 55%)` |

**Status colors:**

| Status | Color | Usage |
|---|---|---|
| Active | `--accent` (green) | Active clients, completed tasks |
| Pending | `--warning` (amber) | Pending payments, in-progress tasks |
| Overdue | `--destructive` (red) | Overdue tasks, failed transactions |
| Inactive | `--muted` (gray) | Inactive clients, archived items |

### Typography

```css
/* globals.css */
:root {
  --font-sans: "Inter", system-ui, -apple-system, sans-serif;
  --font-mono: "JetBrains Mono", ui-monospace, monospace;
}
```

| Element | Size | Weight | Usage |
|---|---|---|---|
| Page title | `text-2xl` (1.5rem) | 600 (semibold) | Page headers |
| Section title | `text-lg` (1.125rem) | 600 | Card headers, section dividers |
| Body | `text-sm` (0.875rem) | 400 | Default body text — compact for data density |
| Table data | `text-sm` (0.875rem) | 400 | Table cells |
| Label | `text-sm` (0.875rem) | 500 (medium) | Form labels, column headers |
| Caption | `text-xs` (0.75rem) | 400 | Timestamps, metadata, secondary info |
| Monetary amounts | `text-sm` / `font-mono` | 500 | Payment amounts, financial figures |

Font loaded via `next/font/google`:

```typescript
import { Inter, JetBrains_Mono } from "next/font/google";
const inter = Inter({ subsets: ["latin"], variable: "--font-sans" });
const jetbrainsMono = JetBrains_Mono({ subsets: ["latin"], variable: "--font-mono" });
```

### Responsive Breakpoints

Mobile-first approach using Tailwind defaults:

| Breakpoint | Width | Layout Behavior |
|---|---|---|
| Default (mobile) | `< 640px` | Single column, bottom nav, collapsible panels |
| `sm` | `≥ 640px` | Two-column forms, wider cards |
| `md` | `≥ 768px` | Sidebar appears (collapsed), table views |
| `lg` | `≥ 1024px` | Sidebar expanded, multi-column dashboard |
| `xl` | `≥ 1280px` | Full dashboard grid, side panels |

### Admin Layout (Mobile Responsive)

```
Desktop (lg+):                         Mobile (<md):
┌──────┬────────────────────────┐      ┌────────────────────────┐
│      │  Header + Breadcrumbs  │      │ ☰ Header + Breadcrumbs │
│ Side │────────────────────────│      │────────────────────────│
│ bar  │                        │      │                        │
│      │     Page Content       │      │     Page Content       │
│ Nav  │                        │      │     (full width)       │
│      │                        │      │                        │
│      │                        │      │────────────────────────│
└──────┴────────────────────────┘      │  ◆  ◆  ◆  ◆  Bottom  │
                                       └────────────────────────┘
```

- **Desktop**: Persistent sidebar (collapsible to icon-only)
- **Tablet**: Sidebar collapses to hamburger overlay
- **Mobile**: Bottom navigation bar with 4-5 primary destinations; hamburger menu for secondary items

### Client Portal Layout (Mobile Responsive)

```
Desktop (lg+):                         Mobile (<md):
┌────────────────────────────────┐     ┌────────────────────────┐
│  Logo    Nav Links    Profile  │     │ Logo           ☰ Menu  │
│────────────────────────────────│     │────────────────────────│
│                                │     │                        │
│        Page Content            │     │     Page Content       │
│        (centered max-w)        │     │     (full width)       │
│                                │     │                        │
│────────────────────────────────│     │────────────────────────│
│           Footer               │     │         Footer         │
└────────────────────────────────┘     └────────────────────────┘
```

- Simple top nav with logo, 3 links (Messages, Documents, Payments)
- Centered content with `max-w-4xl` on desktop
- Full-width on mobile with generous padding

### Key Component Patterns

**DataTable** (responsive):
- Desktop: Full table with sorting, filtering, pagination
- Mobile: Switches to card-based list view with key fields visible, expandable for details

**Dashboard Widgets** (responsive grid):
- Desktop: 3-4 column grid
- Tablet: 2 column grid
- Mobile: Single column, stacked cards

**Forms** (responsive):
- Desktop: Multi-column layout for dense forms (2-3 columns)
- Mobile: Single column, stacked fields

**Navigation**:
- Notification bell with unread count badge — visible at all breakpoints
- Quick-search (Cmd+K / Ctrl+K) — desktop only; search icon on mobile

### Dark Mode

- Supported via shadcn/ui built-in dark mode (class-based toggle)
- User preference persisted per user
- Default: system preference

### Portal Branding

The client portal uses the same shadcn/ui components but with a distinct visual identity:
- Custom `--primary` hue configurable per tenant (stored in `Tenant.Settings`)
- Tenant logo in the header
- Simplified component set (no admin-specific components like Kanban boards)

---

## 10. Integrations Architecture

### Google Workspace (Gmail, Calendar, Drive)

- OAuth 2.0 per-user consent against the firm's Google Workspace account
- Tokens stored encrypted in Azure SQL, refreshed automatically
- **Gmail**: Periodic sync via `BackgroundService` (configurable interval) + webhook push notifications for real-time
- **Calendar**: Read events from all team members, display in unified dashboard view
- **Drive**: CRM manages folder structure programmatically; documents stored in Drive with metadata in SQL

### Azure OpenAI Service

- Used exclusively for email drafting assistance
- System prompt enforces professional tone, tax consulting context
- All processing within the same Azure tenant/region — no data leaves the boundary
- AI output always requires human review before sending

### Notification Engine

- Centralized `INotificationService` called by all modules (registered in `AddInfrastructure`)
- Channels: In-app (SignalR real-time) + Email (Microsoft Graph API)
- Per-user preferences stored in `NotificationPreference` table
- Supports immediate delivery and daily digest batching

---

## 11. Security Architecture

| Concern | Implementation |
|---|---|
| Authentication | Microsoft Entra ID (OpenID Connect + MSAL) |
| Authorization | Role-based (Admin/Associate/Client) + client-assignment-based data filtering |
| Data isolation | EF Core global query filters on TenantId; client assignment checks on every query |
| Encryption at rest | Azure SQL TDE (AES-256) + Azure Key Vault for application secrets |
| Encryption in transit | TLS 1.3 enforced on all endpoints |
| Payment data | PCI DSS v4.0 — tokenized via payment partner; raw card numbers never touch our database |
| Session management | JWT tokens with 30-minute sliding expiration; MFA required for admin accounts |
| Audit trail | Every data modification logged to AuditLog table via EF Core interceptor |
| Secret management | Azure Key Vault — no secrets in code, config files, or environment variables |
| Backend validation | FluentValidation on all request models (explicit per-endpoint) |
| Frontend validation | Zod schemas on all forms + `codeRegex`/`urlRegex` refine on text inputs |
| Error responses | `ProblemDetails` with `errorCode` — no stack traces in production |

---

## 12. Phased Delivery Map

| Phase | Modules | External Integrations |
|---|---|---|
| **MVP** | Client Management, Client Portal, Document Management, Communications (templates + Gmail mirror), Dashboard, Notification Engine, RBAC | Entra ID, Gmail API, Google Calendar API, Google Drive API, Azure OpenAI, Microsoft Graph API |
| **Phase 2** | Payment Collection, Task/Workflow Engine, Internal Messaging + Escalation | Payment Partner API |
| **Phase 3** | Marketing/Newsletter Engine, Dashboard Enhancements | (Microsoft Graph campaigns) |
| **Phase 4** | QuickBooks Online Sync, Full Bilingual UI (EN/ES) | QBO API (Intuit) |
| **Phase 5** | Expert Tax Integration (optional), Advanced Workflow Automation | Expert Tax (TBD) |

---

## 13. Architecture Decision Records

Significant decisions are tracked in `docs/adr/` using the format:

```
docs/adr/
  001-monorepo-structure.md
  002-clean-architecture-custom-cqrs.md
  003-multi-tenancy-strategy.md
  004-google-drive-as-storage.md
  005-minimal-apis-over-controllers.md
  006-tailwind-shadcn-over-bootstrap.md
  007-otel-over-serilog-direct.md
```

Each ADR follows the format: **Context → Decision → Consequences**.
