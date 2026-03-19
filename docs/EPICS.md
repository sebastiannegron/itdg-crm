# ITDG CRM — Epic, Feature, Story & Task Breakdown

> **Version:** 2.0 — Updated to match ITDG standards (Minimal APIs, custom CQRS, 4-file page convention)
> **Scope:** MVP (Phase 1) only. Phase 2–5 breakdowns will be added when those phases are planned.
> **Convention:** Each Task maps to exactly one GitHub Issue, scoped for a single PR.
> **Labels:** `epic`, `feature`, `story`, `task`, `phase:mvp`, `module:{name}`

---

## Epic 0: Project Foundation & Infrastructure

> Establish the repository, CI/CD pipeline, base project structure, and shared infrastructure that all modules depend on.

### Feature 0.1: Repository & Tooling Setup

#### Story 0.1.1: As a developer, I can clone the repo and build both projects with a single command

- **Task 0.1.1.1** — Initialize .NET solution with Clean Architecture projects
  > Create `Itdg.Crm.sln` with five projects: `Itdg.Crm.Api` (host), `Itdg.Crm.Api.Application`, `Itdg.Crm.Api.Infrastructure`, `Itdg.Crm.Api.Domain`, and `Itdg.Crm.Api.Diagnostics`. Add `Directory.Build.props` (nullable enabled, .NET 9), `global.json`, and `GlobalUsings.cs` per project. Wire project references per dependency flow. Verify `dotnet build` succeeds.

- **Task 0.1.1.2** — Initialize Next.js 16 project with TypeScript and Tailwind
  > Create `src/web/` with Next.js 16, React 19, TypeScript strict mode (ESM), Tailwind CSS 4, and shadcn/ui. Add `instrumentation.ts` and `instrumentation.node.ts` stubs for OpenTelemetry. Configure `next.config.ts`, `tsconfig.json`, `tailwind.config.ts` with CRM theme (see ARCHITECTURE.md Section 9). Add Inter and JetBrains Mono fonts via `next/font/google`. Verify `npm run build` succeeds.

- **Task 0.1.1.3** — Add shared repo tooling files
  > Add `.editorconfig`, `.gitignore` (for .NET + Node), `.prettierrc`, `CLAUDE.md` (project-level instructions). Ensure all generated/build artifacts are ignored.

- **Task 0.1.1.4** — Initialize test project
  > Create `src/api/Itdg.Crm.Api.Test/` with xUnit + FluentAssertions + NSubstitute references. Organize with folders: `Commands/`, `Queries/`, `Endpoints/`, `Repositories/`. Create `src/web/tests/` with Vitest + RTL config. Add a placeholder test in each. Verify `dotnet test` and `npm test` pass.

#### Story 0.1.2: As a developer, my PR is automatically built and tested by CI

- **Task 0.1.2.1** — Create GitHub Actions CI workflow for the .NET backend
  > `.github/workflows/api-ci.yml`: trigger on PRs touching `src/api/**`. Steps: restore, build, test, report results.

- **Task 0.1.2.2** — Create GitHub Actions CI workflow for the Next.js frontend
  > `.github/workflows/web-ci.yml`: trigger on PRs touching `src/web/**`. Steps: install, lint, type-check, test, build.

### Feature 0.2: Infrastructure as Code

#### Story 0.2.1: As a DevOps engineer, I can deploy all Azure resources from Bicep templates

- **Task 0.2.1.1** — Create Bicep modules for core Azure resources
  > Create `infra/modules/` with Bicep files for: App Service (API), App Service (Web), Azure SQL Database, Key Vault, Application Insights. Create `infra/main.bicep` that composes them. Add `infra/parameters/dev.bicepparam`.

- **Task 0.2.1.2** — Create deployment GitHub Action
  > `.github/workflows/deploy.yml`: manual trigger + auto-deploy on push to `main`. Deploy Bicep, then deploy API and Web apps.

### Feature 0.3: Base Domain & Persistence

#### Story 0.3.1: As a developer, I have base entity classes and a working DbContext

- **Task 0.3.1.1** — Create base entity classes in Domain
  > Create `BaseEntity` (Id as Guid, CreatedAt, UpdatedAt), `TenantEntity` (adds TenantId), and `ISoftDeletable` interface (DeletedAt). Add to `Itdg.Crm.Api.Domain/Entities/`. Create `IGenericRepository<T>` interface in `Itdg.Crm.Api.Domain/Repositories/`.

- **Task 0.3.1.2** — Create CrmDbContext with interceptors
  > Create `IApplicationDbContext` interface in Application and `CrmDbContext` implementation in `Itdg.Crm.Api.Infrastructure/Data/`. Add interceptors for: automatic `CreatedAt`/`UpdatedAt` timestamps, soft-delete filtering, and TenantId global query filter. Register via `AddDbContext` in `AddInfrastructure`.

- **Task 0.3.1.3** — Create Tenant entity and configuration
  > Create `Tenant` entity in Domain. Create EF configuration in Infrastructure. Add seed data migration with a default tenant for development.

- **Task 0.3.1.4** — Create ITenantProvider and GenericRepository
  > Create `ITenantProvider` interface in Application (returns current tenant ID from claims). Implement `ClaimsTenantProvider` in Infrastructure. Create `GenericRepository<T>` in `Itdg.Crm.Api.Infrastructure/Repositories/`. Register all in `AddInfrastructure`.

### Feature 0.4: Authentication & Authorization

#### Story 0.4.1: As a user, I can log in with Microsoft Entra ID

- **Task 0.4.1.1** — Configure Entra ID authentication on the .NET API
  > Add `Microsoft.Identity.Web` package. Configure JWT bearer auth in `Program.cs` using `AddMicrosoftIdentityWebApiAuthentication`. Add `appsettings.json` placeholders for `AzureAd` section. Create `AzureAdOptions` with `const string Key` and `[Required]` annotations. Add `RequireAuthorization` globally.

- **Task 0.4.1.2** — Configure MSAL.js authentication on Next.js
  > Install `@azure/msal-browser` and `@azure/msal-react`. Create `server/Services/auth-config.ts` with MSAL config. Create auth provider component in `app/_components/AuthProvider.tsx`. Create login/logout pages at `app/auth/`. Protect `(admin)` routes via `i18n/middleware.ts`.

- **Task 0.4.1.3** — Create User entity and sync from Entra ID claims
  > Create `User` entity (Id, TenantId, EntraObjectId, Email, DisplayName, Role, IsActive) in Domain. On first login, auto-create user record from JWT claims if not exists. Create EF configuration in Infrastructure. Create `IUserRepository` and `UserRepository`.

#### Story 0.4.2: As an admin, RBAC is enforced so associates only see their assigned clients

- **Task 0.4.2.1** — Create authorization policies
  > Create role-based authorization policies for endpoint groups. Use `.RequireAuthorization(policy => policy.RequireRole(...))` pattern on route groups. Create `ClientAssignmentAuthorizationHandler` that checks if the current user is assigned to the requested client. Register in `AddInfrastructure`.

- **Task 0.4.2.2** — Create ClientAssignment entity and management
  > Create `ClientAssignment` entity (ClientId, UserId, AssignedAt). Create EF configuration. Create `AssignClient` command + `AssignClientHandler` and `UnassignClient` command + `UnassignClientHandler` in Application layer. Register handlers in `AddInfrastructure`.

### Feature 0.5: Shared Backend Infrastructure

#### Story 0.5.1: As a developer, I have the custom CQRS abstractions and observability ready

- **Task 0.5.1.1** — Create custom CQRS abstractions
  > Create `ICommand`, `IQuery<TResult>`, `ICommandHandler<TCommand>`, `IQueryHandler<TQuery, TResult>`, and `ICacheService` interfaces in `Itdg.Crm.Api.Application/Abstractions/`. Follow ITDG signature: `HandleAsync(command, language, correlationId, cancellationToken)`.

- **Task 0.5.1.2** — Create DiagnosticsConfig and OpenTelemetry setup
  > Create `DiagnosticsConfig` class with `ActivitySource` in `Itdg.Crm.Api.Diagnostics/`. Configure OpenTelemetry in `Program.cs` with Azure Monitor exporter. Add request correlation middleware that reads/generates `X-Correlation-Id` header.

- **Task 0.5.1.3** — Create domain exception base classes
  > Create base `DomainException` with `ErrorCode` property in `Itdg.Crm.Api.Application/Exceptions/`. Create common exceptions: `NotFoundException`, `ForbiddenException`, `ConflictException`. Each carries a typed `ErrorCode` string for `ProblemDetails` responses.

- **Task 0.5.1.4** — Create AddInfrastructure DI entry point
  > Create `AppExtensions.cs` in `Itdg.Crm.Api.Infrastructure/Extensions/` with a single `AddInfrastructure(IServiceCollection, IConfiguration)` method. Register DbContext, repositories, handlers, and external service clients here. Call from `Program.cs`. Register FluentValidation validators via `AddValidatorsFromAssemblyContaining`.

### Feature 0.6: Frontend Shell & Layout

#### Story 0.6.1: As a user, I see a branded application shell with navigation

- **Task 0.6.1.1** — Create admin layout with responsive sidebar navigation
  > Create `app/[locale]/(admin)/layout.tsx` with a navy (`#1a2744`) sidebar using R&A branding (orange `#E85320` brand name + muted white subtitle). Desktop (lg+): 210px persistent sidebar (not collapsible). Tablet (md–lg): 52px icon-only persistent sidebar with tooltips. Mobile: 56px navy bottom nav bar. Nav items: Dashboard, Clients, Documents, Communications, Tasks — Settings as gear icon in sidebar footer. Active state: orange left border + `rgba(232,83,32,0.18)` background + white text. Header: 50px with notification bell (unread badge support) and user avatar (navy bg, white initials). Reference: `docs/ui-template.tsx`.

- **Task 0.6.1.2** — Create client portal layout
  > Create `app/[locale]/(portal)/layout.tsx` with a branded portal header (tenant logo, top nav: Messages, Documents, Payments placeholder). Visually distinct from admin layout — centered content with `max-w-4xl`, simple top nav, full-width on mobile. Support tenant-customizable primary color.

- **Task 0.6.1.3** — Create shared UI components
  > Set up shadcn/ui with CRM theme tokens (colors, typography from ARCHITECTURE.md Section 9). Create shared components in `app/_components/`: `DataTable` (table on desktop, card list on mobile, with sorting/filtering/pagination), `PageHeader`, `EmptyState`, `LoadingSpinner`, `ErrorBoundary`, `TierBadge` (tier 1/2/3 with gold/blue/gray color schemes), `StatusBadge` (Active/Pending Docs/Awaiting Payment + task statuses with specific color mappings), `NotificationDot` (8px colored dot: doc=blue, alert=red, task=green, msg=purple). See `docs/ui-template.tsx` for exact color specs.

- **Task 0.6.1.4** — Create base API client and service layer
  > Create `server/Services/api-client.ts` (typed fetch wrapper that attaches MSAL auth tokens to .NET API calls). Create example service function in `server/Services/dashboardService.ts`. Create example server action with OTel span wrapping. Verify end-to-end connectivity to the .NET API.

- **Task 0.6.1.6** — Add Tasks nav placeholder and Coming Soon page
  > Add Tasks nav item (CheckSquare icon) to admin sidebar. Create a placeholder page at `app/[locale]/(admin)/tasks/page.tsx` displaying "Coming Soon — Phase 2" message. The full Tasks Kanban board (from `docs/ui-template.tsx`) maps to Phase 2 per current roadmap.

- **Task 0.6.1.5** — Configure next-intl and shared locale files
  > Install `next-intl`. Create `i18n/routing.ts` with `en-pr` / `es-pr` locales and `createNavigation`. Create `i18n/request.ts`. Create `app/[locale]/_shared/app-fieldnames.ts` with typed i18n dictionary for both locales (initial: nav strings). Create `app/[locale]/_shared/app-enums.ts` with `PageStatus` type, `codeRegex`, `urlRegex`. Configure `middleware.ts` for locale detection.

---

## Epic 1: Client Management

> Core client records — create, edit, search, classify by tier, assign associates. The foundation that all other modules reference.

### Feature 1.1: Client CRUD

#### Story 1.1.1: As an admin, I can create a new client record

- **Task 1.1.1.1** — Create Client and ClientTier domain entities
  > Create `Client` entity (Name, ContactEmail, Phone, Address, TierId, Status, IndustryTag, Notes, CustomFields as JSON). Create `ClientTier` entity (Name, SortOrder). Create enums in `Itdg.Crm.Api.Domain/GeneralConstants/`: `ClientStatus` (Active, Inactive, Suspended). Create `IClientRepository` in Domain. Add EF configurations in Infrastructure. Create `ClientRepository`.

- **Task 1.1.1.2** — Create client CRUD commands, queries, and handlers
  > In Application layer: Create commands `CreateClient`, `UpdateClient`, `DeleteClient` (soft delete) in `Commands/`. Create handlers `CreateClientHandler`, `UpdateClientHandler`, `DeleteClientHandler` in `CommandHandlers/`. Create queries `GetClientById`, `GetClients` (with pagination, filtering by tier/status/search) in `Queries/`. Create handlers `GetClientByIdHandler`, `GetClientsHandler` in `QueryHandlers/`. Create `ClientDto`, `ClientCreatedDto` in `Dtos/`. Register all handlers in `AddInfrastructure`.

- **Task 1.1.1.3** — Create ClientsEndpoints with Minimal API routes
  > Create `ClientsEndpoints` static class in `Itdg.Crm.Api/Endpoints/` with `MapClientsEndpoints` extension. Routes: `GET /api/v1/Clients`, `GET /api/v1/Clients/{client_id}`, `POST /api/v1/Clients`, `PUT /api/v1/Clients/{client_id}`, `DELETE /api/v1/Clients/{client_id}`. Create request models + FluentValidation validators in `Requests/`. Enforce admin-only for create/delete; assignment-based for read. Per-endpoint try-catch with correlation ID.

- **Task 1.1.1.4** — Create client list page (frontend)
  > Create 4-file page at `app/[locale]/(admin)/clients/`: `page.tsx` (server component fetching clients via `clientService.ts`), `ClientsView.tsx` (client component with DataTable showing Name, Tier badge, Status, Assigned Associate, actions), `shared.ts` (filter schemas), `actions.ts` (OTel-wrapped server actions). Add search input and tier/status filter dropdowns. Responsive: table on desktop, card list on mobile.

- **Task 1.1.1.5** — Create client detail/edit page (frontend)
  > Create 4-file page at `app/[locale]/(admin)/clients/[client_id]/`: `page.tsx` (fetch client by ID via `clientService.ts`), `ClientDetailView.tsx` (tabbed layout: Overview with profile fields, Documents placeholder, Communications placeholder, Tasks placeholder), `shared.ts` (Zod schemas with `codeRegex`/`urlRegex` validation on text inputs, locale-aware error messages), `actions.ts` (update client action with OTel span). Create similar 4-file page at `clients/new/` for creation form. Use react-hook-form + zodResolver.

#### Story 1.1.2: As an admin, I can assign tiers and associates to clients

- **Task 1.1.2.1** — Create tier management endpoints and UI
  > Create `TiersEndpoints` with: `GET /api/v1/Tiers`, `POST /api/v1/Tiers`, `PUT /api/v1/Tiers/{tier_id}` (admin only). Create commands, handlers, request models. Seed default tiers (Tier 1, Tier 2, Tier 3). Register in `AddInfrastructure`. Add tier management section to Settings page using 4-file convention.

- **Task 1.1.2.2** — Create client assignment UI
  > Add associate assignment panel to client detail page. Dropdown to select associate(s) from user list. Call `POST /api/v1/Clients/{client_id}/Assignments` and `DELETE /api/v1/Clients/{client_id}/Assignments/{user_id}` via server actions.

#### Story 1.1.3: As an associate, I can only see clients assigned to me

- **Task 1.1.3.1** — Enforce client assignment filtering in queries
  > Modify `GetClientsHandler` and `GetClientByIdHandler` to filter by `ClientAssignment` when the current user is an Associate (resolved via `ITenantProvider` / user claims). Admin sees all. Write unit tests in `Itdg.Crm.Api.Test/Queries/` verifying data segregation.

### Feature 1.2: Client Timeline

#### Story 1.2.1: As a user, I can see all activity for a client in a unified timeline

- **Task 1.2.1.1** — Create client timeline query and endpoint
  > Create `GetClientTimeline` query and handler that aggregates recent documents, messages, emails, tasks, and payment transactions for a client into a chronological feed. Create endpoint in `ClientsEndpoints`: `GET /api/v1/Clients/{client_id}/Timeline`. Create `TimelineItemDto` in `Dtos/`.

- **Task 1.2.1.2** — Create client timeline UI component
  > Create `ClientTimeline` component in the client detail page folder showing activity items (icon, type, description, timestamp, actor). Add to the Overview tab. Include "load more" via server action. Responsive: compact card layout on mobile.

---

## Epic 2: Document Management

> Upload, organize, search, and version-control client documents. Integrates with Google Drive as the storage backend.

### Feature 2.1: Google Drive Integration

#### Story 2.1.1: As an admin, I can connect the firm's Google Drive account

- **Task 2.1.1.1** — Create Google Drive API client service
  > Create `IGoogleDriveService` interface in `Application/Abstractions/`. Implement `GoogleDriveService` in `Infrastructure/Services/` using the Google Drive API SDK. Support: create folder, upload file, download file, list files, delete file. Handle OAuth 2.0 token refresh. Create `GoogleDriveOptions` with `const string Key`. Register in `AddInfrastructure`.

- **Task 2.1.1.2** — Create Google OAuth consent flow
  > Create `IntegrationsEndpoints` with `GET /api/v1/Integrations/Google/Auth` (redirect to Google consent) and `GET /api/v1/Integrations/Google/Callback` (exchange code for tokens). Store encrypted tokens in the database per user. Create settings page UI to initiate connection.

- **Task 2.1.1.3** — Create automatic folder structure creation
  > When a client is created (in `CreateClientHandler`), automatically create the Google Drive folder hierarchy via `IGoogleDriveService`: `{ClientName}/` → `{Year}/` → one subfolder per document category. Use the configured naming conventions from `DocumentCategory` records.

### Feature 2.2: Document CRUD

#### Story 2.2.1: As an associate, I can upload and organize client documents

- **Task 2.2.1.1** — Create Document and DocumentCategory domain entities
  > Create `Document` entity (ClientId, CategoryId, FileName, GoogleDriveFileId, UploadedById, CurrentVersion, FileSize, MimeType). Create `DocumentCategory` entity (Name, NamingConvention, IsDefault). Create `DocumentVersion` entity. Create `IDocumentRepository` in Domain. Add EF configurations in Infrastructure. Create `DocumentRepository`. Seed default categories (Bank Statements, Invoices, Reports, Tax Documents, Contracts, General). Register in `AddInfrastructure`.

- **Task 2.2.1.2** — Create document upload command and endpoint
  > Create `UploadDocument` command + `UploadDocumentHandler`: validate file type/size, enforce naming convention, upload to Google Drive via `IGoogleDriveService`, create Document and DocumentVersion records. Create endpoint in `DocumentsEndpoints`: `POST /api/v1/Clients/{client_id}/Documents` (multipart upload). Create request model + validator.

- **Task 2.2.1.3** — Create document list and download endpoints
  > Create `GetClientDocuments` query (filter by category, year, search) and `DownloadDocument` query (returns Google Drive download URL) with handlers. Create endpoints: `GET /api/v1/Clients/{client_id}/Documents` and `GET /api/v1/Documents/{document_id}`. Enforce client assignment access in handlers.

- **Task 2.2.1.4** — Create document management UI
  > Create 4-file page at `app/[locale]/(admin)/documents/`: `page.tsx` (fetch documents), `DocumentsView.tsx` (file browser: client filter, category tree, document list with name/size/date/uploader), `shared.ts` (filter schemas), `actions.ts` (upload action with OTel span). Add drag-and-drop upload zone. Responsive: simplified list on mobile.

- **Task 2.2.1.5** — Create document detail view with version history
  > Create document detail panel component showing metadata, version history list (version number, uploader, date), and download button. Add "Upload New Version" action. Add to client detail Documents tab.

#### Story 2.2.2: As an admin, documents are versioned and auditable

- **Task 2.2.2.1** — Create document audit trail
  > Log all document access (view, download, delete) to the AuditLog table via explicit logging in handlers (use `IAuditService`). Create endpoint in `DocumentsEndpoints`: `GET /api/v1/Documents/{document_id}/Audit` (admin only).

- **Task 2.2.2.2** — Implement soft-delete with recycle bin
  > Implement soft-delete for documents (set `DeletedAt` via `ISoftDeletable`). Create `GetRecycleBin` query + handler and `RestoreDocument` command + handler. Add endpoints: `GET /api/v1/Documents/RecycleBin` (admin only) and `POST /api/v1/Documents/{document_id}/Restore`. Create `BackgroundService` for auto-purge after configurable retention period (default 30 days).

### Feature 2.3: Document Search

#### Story 2.3.1: As a user, I can search across all documents by content and metadata

- **Task 2.3.1.1** — Configure Azure AI Search index for documents
  > Create Azure AI Search index with fields: documentId, clientId, clientName, fileName, category, content (extracted text), uploadedAt. Create `ISearchService` interface in Application and `AzureSearchService` implementation in Infrastructure. Index documents on upload in `UploadDocumentHandler`. Register in `AddInfrastructure`.

- **Task 2.3.1.2** — Create search endpoint and UI
  > Create `SearchDocuments` query + handler. Add endpoint: `POST /api/v1/Documents/Search` with query string, optional filters (client, category, date range). Create search UI component with results showing document name, client, category, and relevance snippet. Add to Documents page.

---

## Epic 3: Communications & Client Portal

> Secure client portal, message templates, Gmail integration, AI drafting, and notification engine.

### Feature 3.1: Communication Templates

#### Story 3.1.1: As an admin, I can create and manage standardized message templates

- **Task 3.1.1.1** — Create CommunicationTemplate domain entity
  > Create `CommunicationTemplate` entity (Category, Name, SubjectTemplate, BodyTemplate, Language, Version, IsActive, CreatedById). Create enum in `GeneralConstants/`: `TemplateCategory` (Onboarding, DocumentRequest, PaymentReminder, TaxSeason, General). Create `ITemplateRepository` in Domain. Add EF configuration. Register in `AddInfrastructure`.

- **Task 3.1.1.2** — Create template CRUD endpoints
  > Create commands `CreateTemplate`, `UpdateTemplate`, `RetireTemplate` + handlers. Create queries `GetTemplates`, `GetTemplateById` + handlers. Create `TemplatesEndpoints` with `MapTemplatesEndpoints`. Admin-only for create/update/retire. Create request models + validators. Register all in `AddInfrastructure`.

- **Task 3.1.1.3** — Create template rendering engine
  > Create `ITemplateRenderer` interface in Application and implementation in Infrastructure that processes merge fields (e.g., `{{client.name}}`, `{{dueDate}}`, `{{taxYear}}`). Add `POST /api/v1/Templates/{template_id}/Render` endpoint that accepts merge data and returns rendered subject/body. Support both English and Spanish templates.

- **Task 3.1.1.4** — Create template management UI
  > Create 4-file page at `app/[locale]/(admin)/communications/templates/`: template list (name, category, language, status). Create template editor page with rich text editing, merge field insertion toolbar, and preview pane. Follow 4-file convention with OTel spans in actions.

#### Story 3.1.2: As an associate, I can send standardized messages to clients using templates

- **Task 3.1.2.1** — Create send-from-template flow
  > Create `SendTemplateMessage` command + handler that renders a template with client data and sends via the portal (creates Message record) and/or email (via `IEmailSender` / Microsoft Graph). Create endpoint and UI flow: template selector → preview rendered message → confirm and send.

### Feature 3.2: Client Portal

#### Story 3.2.1: As a client, I can log in to the portal and view my messages

- **Task 3.2.1.1** — Create portal authentication (invitation-based)
  > Create `InviteClient` command + handler that generates a unique invitation link and sends it via email. Create portal login flow using Entra ID B2C or a dedicated Entra ID app registration for portal users. Create `ClientPortal` role-based authorization on portal endpoint group.

- **Task 3.2.1.2** — Create Message entity and portal messaging endpoints
  > Create `Message` entity (ClientId, SenderId, Direction, Subject, Body, TemplateId, IsPortalMessage, IsRead, Attachments). Create `IMessageRepository`. Create `PortalEndpoints` with `MapPortalEndpoints`: `GET /api/v1/Portal/Messages`, `POST /api/v1/Portal/Messages` (reply), `PUT /api/v1/Portal/Messages/{message_id}/Read`. Create commands, queries, handlers. Register all in `AddInfrastructure`.

- **Task 3.2.1.3** — Create portal messages UI
  > Create 4-file page at `app/[locale]/(portal)/portal/messages/`: inbox-style message list (subject, date, read/unread status). Create message detail view with reply form. Show alerts and team messages. Follow portal layout design. Responsive: full-width on mobile.

#### Story 3.2.2: As a client, I can upload and download documents through the portal

- **Task 3.2.2.1** — Create portal document endpoints
  > Add to `PortalEndpoints`: `GET /api/v1/Portal/Documents` (client's own documents only) and `POST /api/v1/Portal/Documents` (upload — auto-route to correct client folder). Create queries + handlers that enforce client can only access their own documents.

- **Task 3.2.2.2** — Create portal documents UI
  > Create 4-file page at `app/[locale]/(portal)/portal/documents/`: simple document list by category/year and upload form with drag-and-drop. Responsive design.

### Feature 3.3: Gmail Integration

#### Story 3.3.1: As an associate, I can see all Gmail exchanges with a client in one view

- **Task 3.3.1.1** — Create Gmail API client service
  > Create `IGmailService` interface in Application and `GmailService` implementation in Infrastructure using the Gmail API SDK. Support: list messages (with filters), get message detail, send email. Handle OAuth 2.0 per-user tokens. Create `GmailOptions`. Register in `AddInfrastructure`.

- **Task 3.3.1.2** — Create EmailMirror entity and sync job
  > Create `EmailMirror` entity (ClientId, GmailMessageId, GmailThreadId, Subject, From, To, BodyPreview, HasAttachments, ReceivedAt). Create EF configuration. Create `GmailSyncBackgroundService` as a `BackgroundService` with `PeriodicTimer` that periodically fetches new emails matching client email addresses and stores mirror records. Create `GmailSyncOptions` with `Enabled` and `TimeBetweenRuns` properties. Register in `AddInfrastructure`.

- **Task 3.3.1.3** — Create email mirror endpoints and UI
  > Add to `CommunicationsEndpoints`: `GET /api/v1/Clients/{client_id}/Emails` with pagination and search. Create `GetClientEmails` query + handler. Create email timeline component on the client detail Communications tab showing mirrored emails chronologically, with expand to view full body.

- **Task 3.3.1.4** — Create Gmail OAuth connection flow
  > Add Gmail OAuth consent endpoints to `IntegrationsEndpoints`. Create UI in Settings for each user to connect their Gmail account. Store per-user OAuth tokens. Show connection status. Support disconnect/reconnect.

### Feature 3.4: AI-Assisted Email Drafting

#### Story 3.4.1: As an associate, I can get AI help drafting a client email

- **Task 3.4.1.1** — Create Azure OpenAI integration service
  > Create `IAiDraftingService` interface in Application and `AzureOpenAiDraftingService` implementation in Infrastructure using Azure OpenAI SDK. System prompt: professional tax consulting context, bilingual (EN/ES) support. Create `AzureOpenAiOptions`. Register in `AddInfrastructure`.

- **Task 3.4.1.2** — Create AI draft endpoint and UI
  > Create `AiEndpoints` with `POST /api/v1/Ai/DraftEmail` accepting prompt, client context, and language. Create `DraftEmail` command + handler. Create UI component: text area for intent description → "Generate Draft" button → editable preview → "Send" or "Discard". Clear labeling that AI drafts require human review. Wrap in OTel span.

### Feature 3.5: Notification Engine

#### Story 3.5.1: As a user, I receive real-time notifications for important events

- **Task 3.5.1.1** — Create Notification and NotificationPreference entities
  > Create `Notification` entity (UserId, EventType, Channel, Title, Body, Status, DeliveredAt, ReadAt). Create `NotificationPreference` entity (UserId, EventType, Channel, IsEnabled, DigestMode). Create enum in `GeneralConstants/`: `NotificationEventType` (DocumentUploaded, PaymentCompleted, PaymentFailed, TaskAssigned, TaskDueSoon, EscalationReceived, PortalMessageReceived, SystemAlert). Add EF configurations. Create `INotificationRepository`. Register in `AddInfrastructure`.

- **Task 3.5.1.2** — Create centralized INotificationService
  > Create `INotificationService` interface in Application with `SendAsync(userId, eventType, title, body, metadata)`. Implement `NotificationService` in Infrastructure: resolves user preferences, routes to enabled channels (in-app = DB insert, email = `IEmailSender`). Register in `AddInfrastructure`.

- **Task 3.5.1.3** — Create notification endpoints and UI
  > Create `NotificationsEndpoints` with `MapNotificationsEndpoints`: `GET /api/v1/Notifications` (paginated, filter by read/unread), `PUT /api/v1/Notifications/{notification_id}/Read`, `PUT /api/v1/Notifications/ReadAll`. Create queries + handlers. Create notification bell icon in admin layout header with unread count badge. Create dropdown panel showing recent notifications.

- **Task 3.5.1.4** — Create notification preferences UI
  > Create 4-file page at `app/[locale]/(admin)/settings/notifications/` with a matrix: rows = event types, columns = channels (in-app, email). Toggle switches for each. Save via server action calling `PUT /api/v1/Notifications/Preferences`. OTel span wrapping.

- **Task 3.5.1.5** — Add SignalR for real-time in-app notifications
  > Add SignalR hub to the .NET API (`NotificationHub`). When a notification is created with in-app channel, push to connected clients via SignalR. Frontend subscribes to the hub in `hooks/use-notifications.ts` and updates the notification badge in real time.

- **Task 3.5.1.6** — Create Microsoft Graph email delivery integration
  > Create `IEmailSender` interface in Application and `MicrosoftGraphEmailSender` implementation in Infrastructure using the Microsoft Graph SDK. Send emails via a shared mailbox or service account. Support: transactional emails (notification delivery), template-based emails (communication templates). Create `MicrosoftGraphEmailOptions` with `const string Key`. Authenticate using the existing Entra ID app registration with `Mail.Send` permission. Register in `AddInfrastructure`.

---

## Epic 4: Management Dashboard

> The primary landing page with aggregated business metrics, task overview, escalation panel, and integrated Google Calendar.

### Feature 4.1: Dashboard Core

#### Story 4.1.1: As an admin, I see a comprehensive dashboard upon login

- **Task 4.1.1.1** — Create dashboard summary endpoint
  > Create `GetDashboardSummary` query + handler returning: total clients (by status, tier), pending tasks count (by status, priority, assignee), recent escalations, upcoming deadlines (next 7 days), unread notifications count. Create `DashboardSummaryDto` in `Dtos/`. Add endpoint to `DashboardEndpoints`: `GET /api/v1/Dashboard/Summary`. Register in `AddInfrastructure`.

- **Task 4.1.1.2** — Create dashboard page with responsive widget layout
  > Create 4-file page at `app/[locale]/(admin)/dashboard/`: `page.tsx` (fetch summary via `dashboardService.ts`), `DashboardView.tsx` (responsive grid: 4 stat cards in a row — 2x2 on mobile — each with colored accent bar: blue for clients, orange for tasks, green for docs, red for escalated). Main content + 280px right column on desktop. Pending Tasks list with priority dots (red=high, amber=medium, gray=low) and StatusBadge. Escalated Issues panel with red cards (`#FEF2F2` bg, `#FECACA` border). Upcoming Deadlines panel with orange date badges. `shared.ts` (widget types), `actions.ts` (refresh actions with OTel). Reference: `docs/ui-template.tsx`.

#### Story 4.1.2: As an admin, I see all team calendars in a unified view

- **Task 4.1.2.1** — Create Google Calendar integration service
  > Create `IGoogleCalendarService` interface in Application and `GoogleCalendarService` implementation in Infrastructure using Google Calendar API SDK. Support: list events across multiple calendars, create event. Handle per-user OAuth tokens. Create `GoogleCalendarOptions`. Register in `AddInfrastructure`.

- **Task 4.1.2.2** — Create calendar widget endpoint and UI
  > Add to `DashboardEndpoints`: `GET /api/v1/Dashboard/Calendar` (aggregates events from all connected team calendars, date range parameter). Create `GetDashboardCalendar` query + handler. Create calendar widget component showing color-coded events by team member. Responsive: compact agenda view on mobile, full calendar on desktop.

### Feature 4.2: Dashboard Customization (Simplified for MVP)

#### Story 4.2.1: As a user, I can rearrange my dashboard widgets

- **Task 4.2.1.1** — Create dashboard layout persistence
  > Create `DashboardLayout` entity (UserId, WidgetConfigurations as JSON). Create `SaveDashboardLayout` command + handler and `GetDashboardLayout` query + handler. Add endpoints to `DashboardEndpoints`. Create drag-and-drop widget rearrangement on the frontend. Save layout on change via server action.

---

## Epic 5: Admin Settings & User Management

> System configuration, user management, integration settings, and document category configuration.

### Feature 5.1: User Management

#### Story 5.1.1: As an admin, I can manage team members and their roles

- **Task 5.1.1.1** — Create user management endpoints
  > Create `UsersEndpoints` with `MapUsersEndpoints`: `GET /api/v1/Users`, `GET /api/v1/Users/{user_id}`, `PUT /api/v1/Users/{user_id}` (update role, active status), `POST /api/v1/Users/Invite` (send invitation email). Create commands, queries, handlers, request models. Admin only via `.RequireAuthorization`. Register in `AddInfrastructure`.

- **Task 5.1.1.2** — Create user management UI
  > Create 4-file page at `app/[locale]/(admin)/settings/users/`: user list (name, email, role, status, last login). Create user detail/edit page for role and status changes. Add invite user dialog. Follow 4-file convention with OTel spans.

### Feature 5.2: System Configuration

#### Story 5.2.1: As an admin, I can configure document categories and naming conventions

- **Task 5.2.1.1** — Create document category management endpoints and UI
  > Create CRUD endpoints in `DocumentCategoriesEndpoints` for `DocumentCategory`. Create 4-file settings page section to add/edit/reorder categories and set naming convention patterns per category.

#### Story 5.2.2: As an admin, I can manage integration connections

- **Task 5.2.2.1** — Create integrations settings page
  > Create 4-file page at `app/[locale]/(admin)/settings/integrations/`: connection status for Google Workspace (per-user Gmail, Calendar, Drive), Microsoft Graph (email), Azure OpenAI. Show connected/disconnected status, last sync time, and connect/disconnect actions.

### Feature 5.3: Audit Log

#### Story 5.3.1: As an admin, I can view a comprehensive audit log

- **Task 5.3.1.1** — Create AuditLog entity and EF interceptor
  > Create `AuditLog` entity (UserId, EntityType, EntityId, Action, OldValues JSON, NewValues JSON, Timestamp, IpAddress) in Domain. Create `AuditSaveChangesInterceptor` in `Infrastructure/Data/` that automatically logs all entity changes. Add EF configuration. Register interceptor in `AddInfrastructure`.

- **Task 5.3.1.2** — Create audit log endpoint and UI
  > Create `AuditLogsEndpoints` with `GET /api/v1/AuditLogs` (paginated, filterable by entity type, user, date range, action). Create `GetAuditLogs` query + handler. Admin only. Create 4-file page at `app/[locale]/(admin)/settings/audit/` with searchable audit log table. Responsive.

---

## Summary: Issue Count by Epic

| Epic | Features | Stories | Tasks | Module Labels |
|---|---|---|---|---|
| 0 — Foundation | 6 | 9 | 22 | `module:foundation`, `module:infra`, `module:auth` |
| 1 — Client Mgmt | 2 | 3 | 8 | `module:clients` |
| 2 — Documents | 3 | 4 | 10 | `module:documents` |
| 3 — Communications | 5 | 6 | 16 | `module:communications`, `module:portal`, `module:notifications` |
| 4 — Dashboard | 2 | 3 | 4 | `module:dashboard` |
| 5 — Settings | 3 | 4 | 5 | `module:settings`, `module:audit` |
| **Total** | **21** | **29** | **65** | |

---

## Suggested Execution Order

Tasks should be implemented in this order to respect dependencies:

1. **Foundation first** (Epic 0) — all other epics depend on this
   - 0.1.1.1 → 0.1.1.2 → 0.1.1.3 → 0.1.1.4 (can parallelize .NET and Next.js)
   - 0.3.1.1 → 0.3.1.2 → 0.3.1.3 → 0.3.1.4
   - 0.5.1.1 → 0.5.1.2 → 0.5.1.3 → 0.5.1.4 (CQRS abstractions before auth)
   - 0.4.1.1 → 0.4.1.2 → 0.4.1.3 → 0.4.2.1 → 0.4.2.2
   - 0.6.1.1 → 0.6.1.2 → 0.6.1.3 → 0.6.1.4 → 0.6.1.5
   - 0.1.2.1 and 0.1.2.2 (CI) can be done anytime after the init tasks

2. **Client Management** (Epic 1) — other modules need Client entity
3. **Documents** (Epic 2) and **Communications** (Epic 3) — can be parallelized
4. **Dashboard** (Epic 4) — aggregates data from all modules
5. **Settings** (Epic 5) — can be interleaved as needed

---

## GitHub Labels to Create

```
epic              (color: #6B21A8, purple)
feature           (color: #1D4ED8, blue)
story             (color: #0891B2, cyan)
task              (color: #059669, green)
phase:mvp         (color: #F59E0B, amber)
phase:2           (color: #F97316, orange)
module:foundation (color: #6B7280, gray)
module:infra      (color: #6B7280, gray)
module:auth       (color: #DC2626, red)
module:clients    (color: #2563EB, blue)
module:documents  (color: #7C3AED, purple)
module:communications (color: #DB2777, pink)
module:portal     (color: #EC4899, pink)
module:notifications  (color: #F59E0B, amber)
module:dashboard  (color: #10B981, green)
module:settings   (color: #6B7280, gray)
module:audit      (color: #78716C, stone)
backend           (color: #4338CA, indigo)
frontend          (color: #0EA5E9, sky)
fullstack         (color: #8B5CF6, violet)
```
