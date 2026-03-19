# Copilot Coding Agent Instructions — ITDG CRM Platform

> This file instructs GitHub Copilot coding agents on how to write code in this repository.
> Read `docs/ARCHITECTURE.md` for full system context before starting any task.

---

## Project Context

This is a CRM platform for a tax consulting practice in Puerto Rico. It has two main components:

- **Backend API** (`src/api/`) — .NET 9 / ASP.NET Core Minimal API using Clean Architecture + Custom CQRS
- **Frontend** (`src/web/`) — Next.js 16 / React 19 / TypeScript using App Router

The system integrates with Google Workspace (Gmail, Calendar, Drive), Microsoft Entra ID, Azure OpenAI, and Microsoft Graph API (email delivery).

---

## General Rules

1. **Follow existing patterns** — before writing new code, read similar files in the codebase and match the style exactly.
2. **English only** for all code, comments, commit messages, and documentation.
3. **Never commit secrets** — no connection strings, API keys, PATs, or credentials in code or config files.
4. **Keep changes small and focused** — one logical change per PR.
5. **Prefer simple, readable code** over clever abstractions.
6. **Do not add features beyond what the issue asks for** — no speculative future-proofing.
7. **All date/time values** must use `America/Puerto_Rico` timezone for display. Store as UTC in the database.

---

## .NET Backend (`src/api/`)

### Architecture

This project follows **Clean Architecture** with five projects. Respect the dependency rules:

```
Itdg.Crm.Api → Itdg.Crm.Api.Application → Itdg.Crm.Api.Domain
Itdg.Crm.Api → Itdg.Crm.Api.Infrastructure → Itdg.Crm.Api.Application → Itdg.Crm.Api.Domain
Itdg.Crm.Api → Itdg.Crm.Api.Diagnostics
Itdg.Crm.Api.Application → Itdg.Crm.Api.Diagnostics
Itdg.Crm.Api.Test → Itdg.Crm.Api.Infrastructure
```

- `Itdg.Crm.Api.Domain` — **zero NuGet dependencies**. Entities, repository interfaces (`IGenericRepository<T>`), enums, and constants in `GeneralConstants/`.
- `Itdg.Crm.Api.Application` — Custom CQRS handlers, DTOs, domain exceptions. Depends only on Domain + Diagnostics.
- `Itdg.Crm.Api.Infrastructure` — EF Core (`Data/`), repository implementations, external service clients. Single `AddInfrastructure` DI entry point.
- `Itdg.Crm.Api` — Composition root. Minimal API endpoints, hosted services, middleware, request models + validators.
- `Itdg.Crm.Api.Diagnostics` — OpenTelemetry `ActivitySource` definition.

**Never** add a reference from Domain → Application, Domain → Infrastructure, or Application → Infrastructure.

### Coding Standards

- Target: **.NET 9 / C# 13**
- **Nullable reference types enabled** (`<Nullable>enable</Nullable>`)
- **File-scoped namespaces** (`namespace Itdg.Crm.Api.Domain.Entities;`)
- **Global usings** in `GlobalUsings.cs` files per project
- Use `record` types for DTOs and value objects
- Use `const` and `readonly` where possible
- Avoid magic strings — use constants or enums

### Async Rules

- **Async all the way down** — every I/O operation must be async
- **NEVER** use `.Result`, `.Wait()`, or `Task.Run()` to wrap sync-over-async
- Always accept `CancellationToken` on async methods

### Custom CQRS (No MediatR)

ITDG uses a custom CQRS implementation. **Do NOT use MediatR.**

```csharp
// Abstractions (in Application/Abstractions/)
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

**File organization:**
- Commands go in `Application/Commands/` — one record per file
- Command handlers go in `Application/CommandHandlers/` — one handler per file
- Queries go in `Application/Queries/` — one record per file
- Query handlers go in `Application/QueryHandlers/` — one handler per file
- DTOs go in `Application/Dtos/`

**Handler pattern:**
```csharp
public class CreateClientHandler : ICommandHandler<CreateClient>
{
    private readonly IClientRepository _repository;
    private readonly ILogger<CreateClientHandler> _logger;

    public CreateClientHandler(IClientRepository repository, ILogger<CreateClientHandler> logger)
    {
        _repository = repository;
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

### Minimal API Endpoints (No MVC Controllers)

Each domain area is a **static class** with a `Map{Area}Endpoints` extension:

```csharp
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

        return group;
    }

    private static async Task<IResult> GetClientsEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetClients, IEnumerable<ClientDto>> handler,
        ILogger<ClientsEndpoints> logger,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var result = await handler.HandleAsync(new GetClients(), Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get clients | CorrelationId: {CorrelationId}", correlationId);
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "get_clients_failed" } });
        }
    }
}
```

### Request Models and Validation

Request models live in `Itdg.Crm.Api/Requests/` — **separate from commands/queries**:

```csharp
// Request model with Data Annotations for structural validation
public class CreateClientRequest
{
    [JsonPropertyName("name")]
    [Required, StringLength(200, MinimumLength = 2)]
    public required string Name { get; set; }
}

// FluentValidation for business rules — called EXPLICITLY in endpoint
public class CreateClientRequestValidator : AbstractValidator<CreateClientRequest> { }
```

Validators are registered via: `builder.Services.AddValidatorsFromAssemblyContaining<CreateClientRequestValidator>();`

Validation is called **explicitly** in endpoint methods — there is no auto-validation pipeline.

### Error Handling

**Per-endpoint try-catch** with correlation ID logging. Return `ProblemDetails` with `errorCode` extension. **No global exception middleware.**

```csharp
catch (ClientNotFoundException ex)
{
    return Results.Problem(
        detail: ex.Message,
        statusCode: StatusCodes.Status404NotFound,
        extensions: new Dictionary<string, object?> { { "errorCode", ex.ErrorCode } });
}
```

### JSON Serialization

- Use **`System.Text.Json`** — never Newtonsoft
- All JSON properties use **`snake_case`** via `[JsonPropertyName]`
- DTOs use `record` types (immutable); request models use `class` with `required`

```csharp
public record ClientDto(
    [property: JsonPropertyName("client_id")] Guid ClientId,
    [property: JsonPropertyName("name")] string Name
);
```

### Dependency Injection

All DI is centralized in a **single** `AddInfrastructure` extension in the Infrastructure layer. Called from `Program.cs`: `builder.Services.AddInfrastructure(builder.Configuration);`

Register all handlers, repositories, and services there — no other DI registration files.

### Options Pattern

Config sections map to POCOs with `const string Key` and `[Required]` annotations:

```csharp
public class GoogleWorkspaceOptions
{
    public const string Key = "GoogleWorkspace";
    [Required] public required string ClientId { get; set; }
}
```

Registration: `services.AddOptionsWithValidateOnStart<T>().Bind(config.GetSection(T.Key)).ValidateDataAnnotations();`

### Observability

- Every handler wraps logic in `DiagnosticsConfig.ActivitySource.StartActivity("Operation Name")`
- Use `ILogger<T>` for all logging with structured properties and correlation IDs
- Never `Console.WriteLine` or `Debug.WriteLine`

### Multi-Tenancy

- Every entity that stores tenant-scoped data must inherit from `TenantEntity`
- EF Core global query filters handle isolation — do **not** add manual `.Where(x => x.TenantId == ...)` filters
- Tenant resolved from JWT claims via `ITenantProvider`

---

## Next.js Frontend (`src/web/`)

### Stack

- **Next.js 16** with App Router (not Pages Router)
- **React 19** with Server Components by default
- **TypeScript** — strict mode, ESM (`"type": "module"`)
- **Tailwind CSS 4** + **shadcn/ui** for components
- **react-hook-form** + **Zod 4** for forms and validation
- **next-intl 4** for internationalization with `[locale]` routing
- **OpenTelemetry** + Azure Monitor for observability

### 4-File Page Convention

Every page under `app/[locale]/` uses exactly these 4 files:

| File | Purpose | Directive |
|---|---|---|
| `page.tsx` | Server Component — fetches data, renders client component | (none) |
| `{Name}Form.tsx` or `{Name}View.tsx` | Client Component — interactive UI | `"use client"` |
| `shared.ts` | Zod schemas (locale-aware), DTO types | (none) |
| `actions.ts` | Server Actions — validate, call service, OTel wrapped | `"use server"` |

### Routing

All pages are under `app/[locale]/` with `en-pr` and `es-pr` locales:

```typescript
// ALWAYS import from @/i18n/routing — NEVER from next/navigation or next/link
import { Link, redirect, usePathname, useRouter } from "@/i18n/routing";
```

Dynamic route params use **snake_case**: `[client_id]`, `[document_id]`.

### Internationalization

Text lives in `app/[locale]/_shared/app-fieldnames.ts`:

```typescript
export const fieldnames = {
  "en-pr": { clients_title: "Clients", required_error: "This field is required" },
  "es-pr": { clients_title: "Clientes", required_error: "Este campo es requerido" },
} as const;
export type Locale = keyof typeof fieldnames;
```

Use `useLocale()` in client components, `getLocale()` from `next-intl/server` in server components.

### Data Fetching

- **Server Components** fetch data via `server/Services/` functions and pass as props
- **Server Actions** handle mutations — `"use server"` at top of `actions.ts`
- **No TanStack Query** — use server components + server actions
- Service functions in `server/Services/` call the .NET API backend via fetch

### Server Actions Rules

- `"use server"` at top of `actions.ts`; accept `FormData` as input
- Validate with Zod `safeParse()`. Return `{ errors: fieldErrors }` on failure
- Return `{ success: boolean, message: string }` on success/failure
- Never throw — always return an error object
- Wrap entire body in `tracer.startActiveSpan()`. Always call `span.end()` in `finally`

### API Routes

Only for webhooks and external callbacks. Use `Response.json()` (not `NextResponse`). Dynamic segments use `snake_case`. Use `force-dynamic` for auth-dependent routes.

### Form Handling

- State flow: `idle → loading → success | failed` (use `PageStatus` type)
- Setup: `useForm({ resolver: zodResolver(Schema(locale)) })`
- Map server errors back to fields via `setError()`

### Component Conventions

- **Server components**: `export default async function`. Fetch data, pass to client components via props.
- **Client components**: `"use client"` at top. No barrel exports — import directly.
- Shared components in `app/_components/` (prefixed with `_`)
- shadcn/ui primitives in `app/_components/ui/`
- Page-specific components stay in their page folder

### Input Security

All user text inputs must validate against injection patterns:

```typescript
import { codeRegex, urlRegex } from "../_shared/app-enums";
z.string().min(1).refine(val => !codeRegex.test(val) && !urlRegex.test(val), "Invalid input");
```

### Styling

- Use **Tailwind CSS utility classes** — no CSS modules or styled-components
- Follow the existing shadcn/ui theme tokens in `tailwind.config.ts` and `globals.css`
- **Mobile-first responsive design** — start with mobile layout, scale up with breakpoints
- Use `text-sm` (0.875rem) as default body text for data density
- Monetary amounts use `font-mono`
- See `docs/ARCHITECTURE.md` Section 9 for full design system (colors, typography, breakpoints)

### Naming Conventions

| Element | Convention | Example |
|---|---|---|
| Page component | `{Name}Page` | `ClientsPage` |
| Form component | `{Name}Form` | `CreateClientForm` |
| View component | `{Name}View` | `DashboardView` |
| Zod schema | `{Action}Schema(locale)` | `CreateClientSchema(locale)` |
| DTO type | `{Action}Dto` | `CreateClientDto` |
| Server action | camelCase verb+noun | `createClient` |
| Service function | camelCase verb+noun | `getClients`, `createClient` |
| Dynamic route param | `snake_case` | `[client_id]`, `[document_id]` |
| Component file | PascalCase `.tsx` | `ClientsView.tsx` |
| OTel spans | Title Case | `"Create Client"` |

---

## Testing

### Backend Tests (`src/api/Itdg.Crm.Api.Test/`)

- Framework: **xUnit** + **FluentAssertions** + **NSubstitute**
- Test naming: `MethodName_Scenario_ExpectedResult`
- Pattern: **Arrange-Act-Assert** in every test
- Minimum: test the happy path + one failure case for every public method
- Single test project organized by: `Commands/`, `Queries/`, `Endpoints/`, `Repositories/`

### Frontend Tests (`src/web/tests/`)

- Framework: **Vitest** + **React Testing Library**
- Test user-visible behavior, not implementation details
- Mock API calls, not components

### When Writing Tests

- Create test files alongside the code you're implementing
- Do not skip tests or leave TODO placeholders
- Run `dotnet test` (backend) or `npm test` (frontend) before marking a task complete

---

## Dependency Management — CRITICAL

**Frontend (`src/web/`):**
- After adding, removing, or changing ANY dependency in `package.json`, you MUST run `npm install` from the `src/web/` directory to regenerate `package-lock.json`
- ALWAYS commit both `package.json` AND `package-lock.json` together in the same commit
- Never manually edit `package-lock.json` — only `npm install` should modify it
- CI uses `npm ci` which requires `package.json` and `package-lock.json` to be in sync — if they are not, the build will fail
- Before pushing, verify: `cd src/web && npm ci` succeeds locally

**Backend (`src/api/`):**
- After adding NuGet packages, verify `dotnet restore` succeeds
- Before pushing, verify: `cd src/api && dotnet build` succeeds

## Git & PR Conventions

- Branch naming: `feature/{issue-number}-short-description` or `bugfix/{issue-number}-short-description`
- Commit messages in imperative mood: "Add client list endpoint" not "Added client list endpoint"
- Keep commits small and focused — one logical change per commit
- Do not commit `bin/`, `obj/`, `node_modules/`, or build artifacts
- Do not commit `.env` files or `appsettings.Development.json` with real credentials

---

## Security Rules

- Use **Microsoft Entra ID** for authentication — never roll custom auth
- Use **Azure Key Vault** for all secrets
- Validate all user input — never trust client data
- Use parameterized queries or EF Core — **NEVER** concatenate SQL strings
- All payment data must be tokenized via the payment partner — raw card numbers never enter our system
- Enforce RBAC at the data layer, not just UI level
- Backend: `.RequireAuthorization(policy => policy.RequireRole(...))` on every endpoint
- Frontend: `codeRegex` + `urlRegex` validation on all text inputs via Zod `.refine()`

---

## Common Mistakes to Avoid

1. Using MediatR — this project uses custom CQRS (`ICommand`/`IQuery`/handlers)
2. Creating MVC controllers — use Minimal API endpoint classes with `MapGroup`
3. Adding business logic in endpoints — it belongs in command/query handlers
4. Creating multiple DI extension methods — all registration goes in `AddInfrastructure`
5. Forgetting `CancellationToken` on async methods
6. Using `DateTime.Now` instead of `DateTimeOffset.UtcNow`
7. Using Newtonsoft.Json — use `System.Text.Json` with `[JsonPropertyName]`
8. Putting request models in the Application layer — they belong in `Itdg.Crm.Api/Requests/`
9. Importing from `next/navigation` or `next/link` — use `@/i18n/routing`
10. Using TanStack Query — use server components + server actions
11. Forgetting OTel spans in handlers and server actions
12. Adding `"use client"` to a component that doesn't need interactivity
13. Forgetting to add the global tenant filter on a new entity
14. Skipping `codeRegex`/`urlRegex` validation on user text inputs
15. **Forgetting to run `npm install` after changing `package.json`** — CI uses `npm ci` which FAILS if the lock file is out of sync. Always run `npm install` and commit both files together
16. Adding a NuGet package without verifying `dotnet restore` and `dotnet build` succeed
