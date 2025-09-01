# Gadgets Inc

A demo and playground for building a .NET distributed app that explores integrations, AI,  
and cloud-native patterns using .NET Aspire.

## Overview
This project is my test and demo project based on a fictive "Gadgets Inc" company.  
The company context is used to make examples and fun use cases more relatable.  
None of the use cases are based on any current or previous companies or customers I have worked for—they exist purely  
to support examples and demos.

I use this project for testing and playing with new technologies like C#/.NET, integrations, AI,  
Semantic Kernel, and MCP.

## Tech at a glance (high level)
- .NET 9+ SDK
- .NET Aspire (AppHost orchestration, local dashboard, service wiring)
- Docker (for containerized dependencies when used)
- Web frontend and API services built with modern .NET

## Repository layout
- GadgetsInc.sln — Solution entry point
- GadgetsInc.AppHost/ — .NET Aspire AppHost (runs the whole app locally)
- GadgetsInc.Web/ — Web frontend
- GadgetsInc.ApiService/ — API/backend service(s)
- GadgetsInc.ServiceDefaults/ — Shared service wiring and defaults

## Prerequisites
- .NET SDK 9.0 or newer
- Docker Desktop running (for any containerized resources used by the app)
- An editor/IDE of your choice (VS Code, Visual Studio, Rider)

## Quick start
Run the full application via the Aspire AppHost (recommended):

```bash
# Restore and build
dotnet restore
dotnet build

# Start everything via AppHost
dotnet run --project GadgetsInc.AppHost
```
Follow the console output for service URLs and, when available, the Aspire dashboard.  
Ensure Docker is running if any containerized dependencies are needed.

### Run services individually (optional)
You can also run individual services for focused development:

```bash
# Web frontend (Blazor/ASP.NET)
dotnet run --project GadgetsInc.Web

# API service(s)
dotnet run --project GadgetsInc.ApiService
```

## Configuration
- App settings: standard ASP.NET Core configuration via `appsettings.json` and environment-specific files (e.g., `appsettings.Development.json`).
- Environment variables: override configuration as needed for local development.
- Launch settings: per-project `Properties/launchSettings.json` for local profiles.

## Development tips
- Hot reload: `dotnet watch --project <ProjectFolder>` for quick feedback loops.
- Prefer running through AppHost to get service wiring and any local dashboards provided by .NET Aspire.

## Contributing & reuse
- You are welcome to use anything from this project in your own work.
- If you have any questions, suggestions, or contributions, please open an issue or submit a pull request.
- You’re more than welcome to reach out to me if you want to talk about technology, cloud/AI, or .NET.

## Troubleshooting
- Docker must be running if the app references containerized dependencies.
- Free up conflicting ports or change the launch profile if something is already bound.
- If builds fail after switching SDKs, clean artifacts: `dotnet clean` and delete `bin/` and `obj/`.

---

This repository is for learning and experimentation. The "Gadgets Inc" company and scenarios are fictional and exist only to make examples more relatable.
