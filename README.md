# Amalgam

A CLI tool that composes distributed microservices, shared libraries, and UI plugins into a single locally-runnable monolith — no Nexus publishing or container orchestration required.

## The Problem

Your architecture looks like this:

- Backend **microservices**, each in its own Git repo, referencing shared libraries via NuGet (Nexus)
- **Shared libraries** and common services, each in their own repo, published as NuGet packages
- A **dashboard UI** with **plugins**, each plugin in its own repo, installed via npm (Nexus)

To run everything locally you'd normally need to publish packages, manage versioning, and coordinate multiple processes. Amalgam eliminates that friction.

## What Amalgam Does

1. **Redirects NuGet references to local source** — generates `Directory.Build.props` overrides so microservices resolve shared libraries from your local filesystem instead of Nexus. Your `.csproj` files are never modified.
2. **Runs all backend services in one process** — loads each microservice as a module into a single ASP.NET Core host with route prefix isolation (`/service-name/...`).
3. **Links frontend plugins locally** — uses `npm link` to wire plugin repos into the dashboard project so you get live local changes.
4. **Provides a single CLI** — one tool to init, validate, build, run, link, and inspect your entire system.

## Quick Start

```bash
# Install as a global tool (after publishing)
dotnet tool install --global Amalgam.Cli

# Auto-discover repos from a parent directory
amalgam init --scan-dir C:\repos

# Review and edit the generated config
notepad amalgam.yml

# Validate your configuration
amalgam validate

# Build all backend projects
amalgam build

# Run the unified host
amalgam run

# Link frontend plugins for local development
amalgam link-plugins
```

## Configuration

Amalgam uses an `amalgam.yml` file at your workspace root:

```yaml
repositories:
  - name: order-service
    type: microservice
    path: C:\repos\order-service
    enabled: true
    routePrefix: /orders          # optional, defaults to /order-service

  - name: user-service
    type: microservice
    path: C:\repos\user-service

  - name: common-auth
    type: library
    path: C:\repos\common-auth
    packageName: MyOrg.Common.Auth  # NuGet package name to override

  - name: shared-data
    type: library
    path: C:\repos\shared-data
    packageName: MyOrg.Shared.Data

  - name: dashboard
    type: dashboard
    path: C:\repos\dashboard-ui

  - name: analytics-plugin
    type: plugin
    path: C:\repos\analytics-plugin
    packageName: "@myorg/analytics-plugin"  # npm package name

  - name: reports-plugin
    type: plugin
    path: C:\repos\reports-plugin

backend:
  port: 5000
  environment:
    ASPNETCORE_ENVIRONMENT: Development
    ConnectionStrings__Default: "Server=localhost;Database=DevDb;Trusted_Connection=true"

frontend:
  dashboardPath: C:\repos\dashboard-ui  # or use the dashboard repo entry
  port: 3000
```

### Repository Types

| Type | Description |
|------|-------------|
| `microservice` | A C# service with controllers/endpoints. Must contain a `.csproj` file. Loaded into the unified host. |
| `library` | A shared C# library published as a NuGet package. Its package references are redirected to local source. |
| `dashboard` | The main frontend application. At most one allowed. |
| `plugin` | A frontend plugin installed into the dashboard via npm. |

## CLI Reference

### `amalgam init`

Generate a starter `amalgam.yml` by scanning a directory for Git repositories.

```bash
amalgam init --scan-dir C:\repos
```

Detection heuristics:
- `.csproj` + `Program.cs` = microservice
- `.csproj` without `Program.cs` = library
- `package.json` with react/vue/angular/next/vite dependency = dashboard
- `package.json` without those = plugin

### `amalgam validate`

Check your configuration for errors (missing paths, duplicate names, missing `.csproj` files, multiple dashboards). Reports all issues, not just the first.

### `amalgam build`

Run `dotnet build` on all enabled microservice and library repositories.

### `amalgam run`

Start the unified ASP.NET Core host with all enabled microservices.

```bash
amalgam run                              # use config defaults
amalgam run --port 8080                  # override port
amalgam run --disable user-service       # skip a service
amalgam run --enable analytics-service   # force-enable a service
amalgam run --env API_KEY=abc123         # set environment variables
amalgam run --verbose                    # show detailed config on startup
```

On startup, a health summary table is printed:

```
┌──────────────────────────────────────────────────────────────┐
│                   Amalgam Startup Summary                    │
├─────────────────┬─────────┬──────────────────┬──────────────┤
│ Service         │ Status  │ Route Prefix     │ Load Time    │
├─────────────────┼─────────┼──────────────────┼──────────────┤
│ order-service   │ Loaded  │ /orders          │ 42.3ms       │
│ user-service    │ Loaded  │ /user-service    │ 38.1ms       │
│ common-auth     │ Loaded  │ -                │ 12.0ms       │
│ reports-svc     │ Error   │ /reports         │ 0.0ms        │
│                 │ Run 'amalgam build' first  │              │
└─────────────────┴─────────┴──────────────────┴──────────────┘
```

### `amalgam link-plugins`

Link frontend plugin repos into the dashboard using `npm link`.

```bash
amalgam link-plugins              # link all enabled plugins
amalgam link-plugins --only analytics-plugin  # link one plugin
```

### `amalgam unlink-plugins`

Remove local links and restore plugins from the npm registry.

### `amalgam status`

Print a table showing all configured repositories, their types, and enabled status.

## Integrating Your Microservices

Each microservice needs to implement the `IAmalgamModule` interface so Amalgam can load it into the unified host:

```csharp
using Amalgam.Core.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public class OrderServiceModule : IAmalgamModule
{
    public string ServiceName => "order-service";

    public void ConfigureServices(IServiceCollection services)
    {
        // Register your DI services — same as your Startup.ConfigureServices
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddControllers()
            .AddApplicationPart(typeof(OrderServiceModule).Assembly);
    }

    public void ConfigureApp(IApplicationBuilder app)
    {
        // Configure middleware — same as your Startup.Configure
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
```

### How Local Package Resolution Works

When you have a library like `MyOrg.Common.Auth` that your microservices reference via NuGet:

```xml
<!-- In order-service.csproj (unchanged) -->
<PackageReference Include="MyOrg.Common.Auth" Version="2.1.0" />
```

Amalgam generates a `Directory.Build.props` override that swaps it for a local project reference:

```xml
<!-- Generated at order-service/.amalgam/Directory.Build.props -->
<Project>
  <ItemGroup>
    <PackageReference Remove="MyOrg.Common.Auth" />
    <ProjectReference Include="C:\repos\common-auth\src\Common.Auth.csproj" />
  </ItemGroup>
</Project>
```

Your original `.csproj` is never modified. Run `amalgam build` after generating overrides.

### Route Prefix Isolation

Each microservice is mounted under its route prefix. A controller at `/api/orders` in the `order-service` (with prefix `/orders`) becomes accessible at `/orders/api/orders`. Configure custom prefixes in `amalgam.yml`:

```yaml
- name: order-service
  type: microservice
  path: C:\repos\order-service
  routePrefix: /api/v1/orders    # custom prefix
```

### Shared Libraries

Libraries registered with `type: library` have their `ConfigureServices` called once, and the registered services are available to all microservices. This avoids duplicate singleton registrations.

## Project Structure

```
src/
├── Amalgam.Core/              # Shared models and services
│   ├── Configuration/         # YAML config (AmalgamConfig, loader, validator, scanner)
│   ├── Modules/               # IAmalgamModule interface
│   ├── PackageResolution/     # NuGet → local project reference overrides
│   └── Plugins/               # npm link/unlink service
├── Amalgam.Cli/               # CLI entry point
└── Amalgam.Host/              # Unified ASP.NET Core host
    ├── AmalgamHostBuilder     # Composes modules into one WebApplication
    ├── ModuleLoader           # Assembly loading + IAmalgamModule discovery
    ├── HealthSummary          # Startup status table
    ├── Middleware/             # Route prefix isolation
    └── Diagnostics/           # Actionable error messages
tests/
└── Amalgam.Tests/             # 50 xUnit tests
```

## Requirements

- .NET 8 SDK
- Node.js / npm (for frontend plugin linking)

## License

MIT
