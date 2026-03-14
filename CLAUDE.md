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
- Reactive properties use `RaiseAndSetIfChanged` from ReactiveUI
- Subscriptions are tracked via `Disposables` collection for cleanup
- Components receive ViewModels as Blazor `[Parameter]` with DI fallback via `[Inject]`

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

## Code Style (from CONTRIBUTING.md)

- 4-space indentation, no tabs
- No underscore prefix for member names; use `this` for instance members
- Prefer `var` when type is obvious
- Use C# type aliases (`int`, `string`, not `Int32`, `String`)
- Curly braces required for all blocks, even single-line
- `using` statements inside namespaces
- No `#region` directives
- Style enforced via ReSharper `.DotSettings` file

## Testing

- **Framework:** NUnit 4.4 with bunit for Blazor component testing
- **Mocking:** Moq
- **Test structure** mirrors source project structure
- Integration tests exist but are excluded from CI by default
