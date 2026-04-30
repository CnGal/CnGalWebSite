# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

CnGal 资料站 — a non-profit, wiki-style Chinese Galgame/AVG encyclopedia website. This is the second-generation full rewrite, organized as a .NET monorepo with ~62 projects targeting net10.0 (migrating from net7.0/net8.0). The main site is at `https://www.cngal.org/`.

## Build, and Lint Commands

```bash
# Build the API Server (REST backend)
dotnet build ./CnGalWebSite/CnGalWebSite.APIServer

# Build the new MainSite (Blazor Server, InteractiveServer mode)
dotnet build ./CnGalWebSite/CnGalWebSite.MainSite

# Format code per .editorconfig conventions
dotnet format

# Docker build (each deployable service has its own Dockerfile)
docker build -f ./CnGalWebSite/CnGalWebSite.APIServer/Dockerfile .
```

- **CI**: GitHub Actions (`.github/workflows/dotnet.yml` builds APIServer + Server on PR/push to master; 15 separate workflows handle per-service Docker image publishing)
- **No dedicated linter** — code style is enforced by `.editorconfig` at the solution level

## Architecture

The solution (`CnGalWebSite.sln`) is organized into solution folders grouping projects by subsystem:

### Subsystems (independently deployable)

| Subsystem | Description |
|---|---|
| **Main Site** (主站) | The primary CnGal encyclopedia, comprising an API backend, multiple Blazor frontend hosts, and shared component libraries |
| **Project Site** (企划站) | Project/planning showcase — own API + SSR + WASM |
| **Game Site** (游戏站) | Gaming community site — own API + SSR + WASM |
| **IdentityServer** (开放平台) | Centralized OpenID Connect auth provider, issues JWT tokens with `CnGalAPI`, `FileAPI`, `TaskAPI` scopes |
| **DrawingBed** (图床) | Dedicated image/file hosting service |
| **ExamineService** (审核) | Content review/audit service |
| **TimedTask** (定时任务) | Scheduled background job runner |
| **Expo** (展会) | Convention/event management platform |
| **Kanban / RobotClient** (看板娘) | QQ chatbot with ChatGPT integration |
| **Maui** | Mobile app (MAUI) |

### Main Site Architecture (two generations coexist)

**Legacy generation** (still used in production):
- `CnGalWebSite.APIServer` — REST API (traditional `Startup.cs` pattern, controllers with `[Route("api/entries/[action]")]` attribute routing)
- `CnGalWebSite.Server` + `CnGalWebSite.Shared` + `CnGalWebSite.WebAssembly` — Blazor UI with three hosting models (Server, WASM, Auto)

**New refactored generation** (active development target):
- `CnGalWebSite.MainSite` — Blazor InteractiveServer host (`Program.cs` top-level statements)
- `CnGalWebSite.MainSite.Shared` — pages, layouts, services, components
- `CnGalWebSite.SDK.MainSite` — CQS (Command Query Separation) SDK with typed `HttpClient` per service interface
- `CnGalWebSite.Components` — new reusable component library (DataTables, Cards, Buttons, Inputs, etc.)

**Prefer working in the new `MainSite` / `MainSite.Shared` / `SDK.MainSite` stack unless the task explicitly targets the legacy system.**

### Data Layer

- **Database**: MySQL 8.0 via EF Core 10 (`Microting.EntityFrameworkCore.MySql` / Pomelo)
- **DbContext**: `AppDbContext` in APIServer, extends `IdentityDbContext<ApplicationUser>` with ~60+ `DbSet<T>` properties
- **Repository pattern**: `IRepository<TEntity, TPrimaryKey>` / `RepositoryBase<TEntity, TPrimaryKey>` wraps `DbSet<T>` with CRUD + LINQ query methods. Controllers use repositories directly — there is no service layer for basic CRUD.
- **All FK relationships** use `DeleteBehavior.Restrict` (no cascading deletes)
- **DateTime values** are automatically converted to/from UTC (±8 hours for China timezone)
- **Search**: Meilisearch

### Authentication

- IdentityServer4 (forked `Cnblogs.IdentityServer4.*` packages) issues JWT tokens
- **API Server** validates JWT Bearer tokens with `ApiScope` policy requiring `CnGalAPI` scope
- **MainSite** uses cookie + OIDC hybrid with `CookieOidcRefresher` for silent token refresh
- Roles: `Admin`, `SuperAdmin`, `Editor`, `User`

### DI Conventions

- **Legacy APIServer**: Automated registration — any class ending in `Service` or `Provider` registers as its public interfaces (scoped). Repositories register as transient.
- **New MainSite**: Explicit registration via `AddMainSiteSdk()` extension — each SDK service gets its own typed `HttpClient` with `AccessTokenHandler` (DelegatingHandler for JWT injection). All SDK services are transient.

### Key Middleware (MainSite, in order)

1. Scheme override (forces HTTPS in non-dev)
2. Content-Type guard — rejects POSTs with empty Content-Type (prevents antiforgery crashes)
3. Forwarded headers (reverse proxy)
4. Cache-Control — different strategies: unauthenticated HTML caches 5min on CDN; authenticated/mini-mode uses `private, no-cache, no-store`

### Shared Libraries

| Library | Purpose |
|---|---|
| `CnGalWebSite.DataModel` | Domain entities, ViewModels/DTOs |
| `CnGalWebSite.Core` | Core abstractions and interfaces |
| `CnGalWebSite.Extensions` | C# extension methods |
| `CnGalWebSite.Helper` | Utility/helper classes |
| `CnGalWebSite.EventBus` | RabbitMQ messaging wrapper |
| `CnGalWebSite.HealthCheck` | Health check endpoint models |

## Code Conventions

From `.editorconfig` (in `CnGalWebSite/` subdirectory):
- 4-space indent for C#/VB; 2-space for JSON/XML/YAML/config
- UTF-8 with BOM for `.cs`, `.css`, `.js`, `.json`, `.html`, `.razor` files
- `var` preferred when type is apparent; expression-bodied properties/indexers/accessors
- Braces always required; Allman style (newline before open brace)
- `readonly` fields suggested; PascalCase for const fields
- `public`/`private` before `static`/`virtual`/`override`/`async` in modifier order

## Key NuGet Dependencies

- **UI**: Masa.Blazor 1.11 (Material Design 3), Blazored.LocalStorage/SessionStorage
- **Auth**: IdentityServer4 (forked), JWT Bearer, OpenIdConnect
- **Data**: EF Core 10, Pomelo MySQL provider, Meilisearch
- **Logging**: NLog (legacy Server), Serilog (IdentityServer), Application Insights (APIServer)
- **Messaging**: MailKit (email), RabbitMQ via EventBus
