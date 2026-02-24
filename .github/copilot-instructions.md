# Copilot Instructions for GadgetsInc

## Build, run, and test commands

Use solution-level commands from the repo root unless you are focusing on a single project.

```bash
dotnet restore GadgetsInc.sln
dotnet build GadgetsInc.sln
dotnet run --project GadgetsInc.AppHost
```

For focused runs:

```bash
dotnet run --project GadgetsInc.Web
dotnet run --project GadgetsInc.ApiService
dotnet run --project GadgetsInc.Shipping.McpServer
```

Tests:

- There are currently no dedicated test projects in `GadgetsInc.sln`.
- If/when tests are added, run all tests with `dotnet test GadgetsInc.sln`.
- Run a single test with:

```bash
dotnet test <TestProject.csproj> --filter "FullyQualifiedName~<TestNameFragment>"
```

Linting:

- No repository-specific lint command is currently defined.

## High-level architecture

- `GadgetsInc.AppHost` is the .NET Aspire orchestrator. It wires `apiservice` and `webfrontend`, sets health checks, and makes `webfrontend` wait for `apiservice`.
- `GadgetsInc.ServiceDefaults` is shared infrastructure used by services (`AddServiceDefaults` / `MapDefaultEndpoints`): OpenTelemetry, service discovery, resilience, and dev-only `/health` + `/alive` endpoints.
- `GadgetsInc.Web` is a Blazor Server frontend. It calls backend services using Aspire service discovery (`https+http://apiservice`) rather than hardcoded host/port routing.
- `GadgetsInc.ApiService` is a Minimal API backend with Semantic Kernel integration and OpenAPI/Scalar in development; primary endpoints are `/chat`, `/chat/simple`, `/summary`, and `/compliance`.
- `GadgetsInc.Shipping.McpServer` is a standalone MCP server using stdio transport. It registers Semantic Kernel plugins as MCP tools and exposes a `/health` endpoint.

## Key repository conventions

- Keep Aspire defaults in service entrypoints: service projects that participate in distributed runtime should call `builder.AddServiceDefaults()` and `app.MapDefaultEndpoints()`.
- Prefer service discovery names (`https+http://apiservice`) in HTTP clients instead of localhost URLs, so services continue to work under AppHost orchestration.
- Semantic Kernel functions are exposed via `[KernelFunction]` + `[Description]` and added through `kernelBuilder.Plugins.AddFromType<...>()`; follow this pattern for new tool/function surfaces.
- MCP server setup pattern is: `AddMcpServer().WithStdioServerTransport().WithTools()`, with tool materialization driven from registered kernel plugins.
- Development-only API documentation is the current pattern (`app.MapOpenApi()` and Scalar mappings inside `if (app.Environment.IsDevelopment())`).
