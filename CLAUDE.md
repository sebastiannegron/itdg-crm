# ITDG CRM — Claude Code Project Instructions

## Project Overview
Multi-tenant CRM platform for a tax consulting practice in Puerto Rico.
- **Backend**: .NET 9 Minimal APIs + Clean Architecture + custom CQRS (no MediatR)
- **Frontend**: Next.js 16 (App Router) + Tailwind 4 + shadcn/ui + next-intl
- **Database**: Azure SQL via EF Core
- **Infrastructure**: Azure (App Service, Key Vault, Application Insights)

## Architecture Reference
Full architecture details are in `docs/ARCHITECTURE.md`. Epic/task breakdown in `docs/EPICS.md`.

## Project Structure
```
src/api/          .NET 9 backend (Clean Architecture)
src/web/          Next.js 16 frontend
infra/            Azure Bicep IaC
docs/             Architecture and planning docs
.github/          CI workflows and issue templates
```

## Backend Conventions
- Solution: `src/api/Itdg.Crm.sln`
- Projects: Api (host), Application, Infrastructure, Domain, Diagnostics, Test
- Custom CQRS: ICommand, IQuery<T>, ICommandHandler<T>, IQueryHandler<T,R> — NO MediatR
- Handler signature: `HandleAsync(command, language, correlationId, cancellationToken)`
- All DI in `Infrastructure/Extensions/AppExtensions.cs` → `AddInfrastructure()`
- Minimal API endpoints as static classes with `Map{Area}Endpoints()`
- Per-endpoint try-catch with correlation ID — no global exception middleware
- JSON: System.Text.Json, snake_case via [JsonPropertyName]
- Options pattern: POCO with `const string Key` + `[Required]`

## Frontend Conventions
- 4-file page convention: page.tsx, {Name}View.tsx/{Name}Form.tsx, shared.ts, actions.ts
- Import Link/redirect/usePathname/useRouter from `@/i18n/routing` — NEVER from next/navigation
- Locales: en-pr, es-pr (i18n via next-intl with typed fieldnames dict)
- Server Components fetch data via `server/Services/`; Server Actions for mutations
- All text inputs validated against codeRegex/urlRegex (injection prevention)
- OTel spans wrap every server action

## Build & Test
```bash
# Backend
cd src/api && dotnet build && dotnet test

# Frontend
cd src/web && npm run build && npm test
```

## Git Workflow
- Branch: `feature/AB#1234-short-description` or `bugfix/AB#1234-short-description`
- Commit messages include `AB#1234` work item reference
- PR into develop (squash merge), develop → main for releases
- Never commit to main or develop directly
