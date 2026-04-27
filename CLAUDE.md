# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

COMET Web Community Edition is a Blazor Server-Side web application implementing the ECSS-E-TM-10-25 standard for concurrent engineering design. Built on .NET 10.0 with ReactiveUI (MVVM + Reactive programming) and DevExpress Blazor components.

## Build & Test Commands

```bash
# Restore dependencies (requires DEVEXPRESS_NUGET_KEY and PACKAGE_TOKEN env vars for private feeds)
dotnet restore COMETwebapp.sln

# Build
dotnet build COMETwebapp.sln --no-restore

# Run all unit tests (excluding integration tests)
dotnet test COMETwebapp.sln --no-build --verbosity normal --filter "FullyQualifiedName!~IntegrationTests"

# Run a single test by name
dotnet test COMETwebapp.sln --no-build --filter "FullyQualifiedName~YourTestClassName.YourTestMethodName"

# Run tests with coverage
dotnet test COMETwebapp.sln --no-build --filter "FullyQualifiedName!~IntegrationTests" /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run the application
dotnet run --project COMETwebapp/COMETwebapp.csproj
```

**NuGet feeds:** nuget.org, GitHub Packages (STARIONGROUP, needs `PACKAGE_TOKEN`), DevExpress private feed (needs `DEVEXPRESS_NUGET_KEY`).

There is **no committed `nuget.config`** — feeds are configured per-environment by CI workflows (`.github/workflows/CodeQuality.yml`, `publish-docker-container.yml`) and the Dockerfile. Locally you must add the feeds yourself and export both env vars, or `dotnet restore` will fail on CDP4-COMET-SDK packages.

## CI Quality Gates

PRs are gated by five workflows in `.github/workflows/`: `CodeQuality.yml` (SonarQube), `codeql.yml`, `semgrep.yml`, `nuget-reference-check.yml`, and `publish-docker-container.yml` (triggers on `web-*` tags). Style violations are caught here, not by `dotnet build` — there is no `.editorconfig` in the repo.

## Release Process

NuGet releases are **manual**, not automated: run `release.bat` at the repo root (Release build → `dotnet pack` → push to nuget.org with an API key). CI does not publish packages.

## Solution Structure

| Project | Purpose | License |
|---------|---------|---------|
| **COMET.Web.Common** | Shared Blazor component library, base ViewModels, core services. Published to NuGet as `CDP4.WEB.Common` | Apache 2.0 |
| **COMETwebapp** | Main web application with feature-specific pages, components, and ViewModels | AGPL 3.0 |
| **COMET.Web.Common.Tests** | Unit tests for the common library (NUnit + bunit + Moq) | |
| **COMET.Web.Common.Test** | Test helper library published to NuGet as `CDP4.WEB.Common.Test` | Apache 2.0 |
| **COMETwebapp.Tests** | Unit and component tests for the main application | |

## Architecture

### MVVM + Reactive Pattern
- **ViewModels** implement interfaces (e.g., `ITabsViewModel`) and extend `DisposableObject`
- `DisposableObject` is **defined locally** in `COMET.Web.Common/Utilities/DisposableObject/DisposableObject.cs` (extends `ReactiveObject`) — not from the SDK. All ViewModels in this repo extend it, not `ReactiveObject` directly
- Reactive properties use `RaiseAndSetIfChanged` from ReactiveUI
- Subscriptions are tracked via `Disposables` collection for cleanup
- Components receive ViewModels as Blazor `[Parameter]` with DI fallback via `[Inject]`
- **Razor code-behind**: components use partial classes — `Foo.razor` for markup + `Foo.razor.cs` for logic. Avoid putting non-trivial logic in `@code` blocks

### Core Services
Most features touch one or both of these (in `COMET.Web.Common/Services/SessionManagement/`):
- **`ISessionService`** — manages the open `ISession`, engineering models, and iterations
- **`IAuthenticationService`** — handles login/logout against a CDP4-COMET server

### Dependency Injection
- Services registered via extension methods in `ServiceCollectionExtensions` classes
- `COMET.Web.Common` registers core services; `COMETwebapp` adds application-specific ones

### Application/Feature System
- Each feature (BookEditor, EngineeringModel, ParameterEditor, Viewer, etc.) has its own folder under both `Components/` and `ViewModels/Components/`
- Application metadata defined in `COMETwebapp/Model/Applications.cs`
- Supports tabbed applications with main/side panel layouts
- URL parameter passing for state management

### Key SDK Dependency
Core domain model comes from **CDP4-COMET-SDK** (`CDP4ServicesDal-CE`, `CDP4Web-CE`). The SDK provides the data access layer and domain types for the ECSS-E-TM-10-25 standard.

### Build Flags Worth Knowing
From `COMETwebapp/COMETwebapp.csproj`:
- `ImplicitUsings=enable` — don't add redundant `using System;` etc.
- `InvariantGlobalization=true`
- `BlazorEnableTimeZoneSupport=false` — timezone work must be explicit; `DateTime.Now`/`TimeZoneInfo` won't behave as in a normal .NET app

## Code Style (from CONTRIBUTING.md)

- 4-space indentation, no tabs
- No underscore prefix for member names; use `this` for instance members
- Prefer `var` when type is obvious
- Use C# type aliases (`int`, `string`, not `Int32`, `String`)
- Curly braces required for all blocks, even single-line
- `using` statements inside namespaces
- No `#region` directives
- Style enforced via ReSharper `.DotSettings` files (no `.editorconfig` exists). `dotnet build` and `dotnet format` will **not** catch violations — SonarQube CI does

## Testing

- **Framework:** NUnit 4.4 with bunit for Blazor component testing
- **Mocking:** Moq
- **Test structure** mirrors source project structure
- Integration tests exist but are excluded from CI by default
- **Reusable helpers** ship in `COMET.Web.Common.Test` (NuGet: `CDP4.WEB.Common.Test`): `DevExpressBlazorTestHelper` (call `ConfigureDevExpressBlazor()` on any bunit `TestContext` rendering DX components), `MockedLoggerHelper`, `TaskHelper`. Prefer these over rolling your own setup
- **Nullable is disabled in test projects** (`<Nullable>disable</Nullable>` in both `*.Tests.csproj`), unlike production code — don't be surprised by missing null-annotations in fixtures
