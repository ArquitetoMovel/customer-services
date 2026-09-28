# Repository Guidelines

## Project Structure & Module Organization

This repository contains three .NET solutions: `CustomerManagementService/`, `UserManagementService/`, and `NotificationService/`. Each keeps production projects under `src/`; UserManagement and NotificationService keep tests under `tests/`. Domain models and ports belong in `Domain`, use cases in `Application`, database and broker adapters in `Infrastructure` or `Infra`, and HTTP endpoints in `Api` or `App`. `UserManagement.UI` contains the Blazor interface and its `wwwroot/` assets. Root `compose.yaml` defines local RabbitMQ, MongoDB, PostgreSQL, and service containers; the root image files are architecture diagrams.

## Build, Test, and Development Commands

Run commands from the repository root with the .NET 10 SDK.

- `dotnet build UserManagementService/UserManagementService.sln` builds that service and its tests; substitute either other solution path as needed.
- `dotnet test UserManagementService/UserManagementService.sln` runs its unit and integration test projects. Run `dotnet test NotificationService/NotificationService.sln` for NotificationService.
- `dotnet run --project CustomerManagementService/src/CustomerManagementAppHost` starts the CustomerManagement Aspire host.
- `docker compose up --build` starts the containers defined in `compose.yaml`; use `docker compose down` to stop them.
- `dotnet format UserManagementService/UserManagementService.sln` formats a solution; substitute the solution you changed.

## Coding Style & Naming Conventions

Follow the existing C# style: four-space indentation, file-scoped namespaces where already used, PascalCase for types and public members, and camelCase for locals and parameters. Keep nullable reference types enabled, as in the project files. Name interfaces with `I` (for example, `IAttendanceTicketRepository`) and place implementations beside their infrastructure concerns. Keep Razor pages in `Pages/` and static UI files in `wwwroot/`.

## Testing Guidelines

Tests use xUnit (`[Fact]` and `[Theory]`). Name test classes after the behavior under test, such as `AttendanceTicketTests`, and methods in `Action_ShouldExpectedResult` form. Add or update focused tests for behavior changes; keep broker and database dependencies out of unit tests. No coverage threshold is configured. The NotificationService `UnitTest1` is a placeholder, so replace it with meaningful cases when changing that service.

## Commit & Pull Request Guidelines

Recent commits use mixed styles, including `feat: ...` and short descriptive subjects. Prefer a concise imperative subject, with a `feat:` or `fix:` prefix when appropriate. In pull requests, describe the affected service, behavior change, commands run, and any configuration or migration steps. Link a related issue when one exists; include screenshots for Blazor UI changes.

## Configuration

Treat credentials in `compose.yaml` and `appsettings.json` as local development values. Supply real secrets through environment-specific configuration, and do not commit production credentials.
