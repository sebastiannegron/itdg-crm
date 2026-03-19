# ITDG CRM Platform

Multi-tenant CRM platform for a tax consulting practice in Puerto Rico. Centralizes client management, document collection, task workflows, communications, and payment collection.

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Next.js 16, React 19, TypeScript, Tailwind CSS 4, shadcn/ui |
| Backend | .NET 9, C# 13, Minimal APIs, EF Core, Clean Architecture |
| Database | Azure SQL |
| Infrastructure | Azure App Service, Key Vault, Application Insights |
| IaC | Azure Bicep |
| CI/CD | GitHub Actions |

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (9.0.100+)
- [Node.js 22](https://nodejs.org/) (22.18.0+)
- [npm 10](https://www.npmjs.com/) (10.9.3+)
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) (for infrastructure deployment)

## Getting Started

### Backend

```bash
cd src/api
dotnet restore
dotnet build
dotnet test
dotnet run --project Itdg.Crm.Api
```

The API will be available at `https://localhost:5001` with Swagger UI at `/swagger`.

### Frontend

```bash
cd src/web
npm install
npm run dev
```

The app will be available at `http://localhost:3000`.

## Project Structure

```
src/
  api/              .NET 9 backend
    Itdg.Crm.Api/           Host (Minimal APIs, middleware, DI)
    Itdg.Crm.Api.Application/   CQRS, handlers, DTOs, exceptions
    Itdg.Crm.Api.Infrastructure/ EF Core, repositories, services
    Itdg.Crm.Api.Domain/        Entities, interfaces, constants
    Itdg.Crm.Api.Diagnostics/   OpenTelemetry ActivitySource
    Itdg.Crm.Api.Test/          xUnit tests
  web/              Next.js 16 frontend
infra/              Azure Bicep IaC
docs/               Architecture documentation
```

## Documentation

- [Architecture](docs/ARCHITECTURE.md) — system design, patterns, and conventions
- [Epics & Tasks](docs/EPICS.md) — work breakdown and execution order
