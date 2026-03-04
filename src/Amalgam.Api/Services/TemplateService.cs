using Amalgam.Core.Configuration;

namespace Amalgam.Api.Services;

public class TemplateService
{
    private static readonly Dictionary<string, TemplateInfo> Templates = new()
    {
        ["basic-microservice"] = new TemplateInfo
        {
            Id = "basic-microservice",
            Name = "Basic Microservice Setup",
            Description = "A minimal setup with one microservice, a shared library, and a dashboard project.",
            RepositoryCount = 3,
            Config = new AmalgamConfig
            {
                Repositories = new List<RepositoryConfig>
                {
                    new() { Name = "my-service", Type = RepositoryType.Microservice, Path = "./my-service", Enabled = true },
                    new() { Name = "shared-lib", Type = RepositoryType.Library, Path = "./shared-lib", Enabled = true },
                    new() { Name = "dashboard", Type = RepositoryType.Dashboard, Path = "./dashboard", Enabled = true }
                },
                Backend = new BackendConfig { Port = 5000 },
                Frontend = new FrontendConfig { DashboardPath = "./dashboard", Port = 3000 }
            }
        },
        ["full-stack"] = new TemplateInfo
        {
            Id = "full-stack",
            Name = "Full Stack with Plugins",
            Description = "Complete setup with microservices, libraries, plugins, and dashboard with merge configs.",
            RepositoryCount = 6,
            Config = new AmalgamConfig
            {
                Repositories = new List<RepositoryConfig>
                {
                    new() { Name = "user-service", Type = RepositoryType.Microservice, Path = "./user-service", Enabled = true },
                    new() { Name = "order-service", Type = RepositoryType.Microservice, Path = "./order-service", Enabled = true },
                    new() { Name = "shared-models", Type = RepositoryType.Library, Path = "./shared-models", Enabled = true },
                    new() { Name = "auth-plugin", Type = RepositoryType.Plugin, Path = "./auth-plugin", Enabled = true, PackageName = "@company/auth-plugin" },
                    new() { Name = "reporting-plugin", Type = RepositoryType.Plugin, Path = "./reporting-plugin", Enabled = true, PackageName = "@company/reporting-plugin" },
                    new() { Name = "main-dashboard", Type = RepositoryType.Dashboard, Path = "./main-dashboard", Enabled = true }
                },
                Backend = new BackendConfig
                {
                    Port = 5000,
                    Environment = new Dictionary<string, string>
                    {
                        ["ASPNETCORE_ENVIRONMENT"] = "Development",
                        ["LOG_LEVEL"] = "Debug"
                    }
                },
                Frontend = new FrontendConfig { DashboardPath = "./main-dashboard", Port = 3000 }
            }
        },
        ["library-dev"] = new TemplateInfo
        {
            Id = "library-dev",
            Name = "Library Development",
            Description = "Focused on shared library development with merge configs and a single consuming service.",
            RepositoryCount = 2,
            Config = new AmalgamConfig
            {
                Repositories = new List<RepositoryConfig>
                {
                    new()
                    {
                        Name = "shared-lib", Type = RepositoryType.Library, Path = "./shared-lib", Enabled = true,
                        Merge = new MergeConfig
                        {
                            Sources = new List<string> { "src/generated", "src/static" }
                        }
                    },
                    new() { Name = "consuming-service", Type = RepositoryType.Microservice, Path = "./consuming-service", Enabled = true }
                },
                Backend = new BackendConfig { Port = 5000 }
            }
        }
    };

    public List<TemplateSummary> GetAll()
    {
        return Templates.Values.Select(t => new TemplateSummary
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            RepositoryCount = t.RepositoryCount
        }).ToList();
    }

    public TemplateInfo? GetById(string id)
    {
        return Templates.GetValueOrDefault(id);
    }
}

public class TemplateInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RepositoryCount { get; set; }
    public AmalgamConfig Config { get; set; } = new();
}

public class TemplateSummary
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RepositoryCount { get; set; }
}
