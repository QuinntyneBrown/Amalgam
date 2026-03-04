using Amalgam.Core.Configuration;
using Amalgam.Core.Modules;
using Amalgam.Host;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Amalgam.Tests;

public class TestModule : IAmalgamModule
{
    public string ServiceName => "test-service";
    public void ConfigureServices(IServiceCollection services) { }
    public void ConfigureApp(IApplicationBuilder app) { }
}

public class ModuleLoaderTests
{
    [Fact]
    public void LoadModules_SkipsDisabledRepositories()
    {
        var loader = new ModuleLoader();
        var repos = new[]
        {
            new RepositoryConfig { Name = "disabled-svc", Enabled = false, Type = RepositoryType.Microservice }
        };

        var (modules, results) = loader.LoadModules(repos);

        Assert.Empty(modules);
        Assert.Single(results);
        Assert.Equal(ModuleLoadStatus.Skipped, results[0].Status);
    }

    [Fact]
    public void LoadModules_LibrariesHaveNoRoutePrefix()
    {
        var loader = new ModuleLoader();
        var repos = new[]
        {
            new RepositoryConfig { Name = "shared-lib", Enabled = true, Type = RepositoryType.Library, Path = "lib.dll" }
        };

        var (modules, results) = loader.LoadModules(repos);

        Assert.Empty(modules);
        Assert.Single(results);
        Assert.Equal(ModuleLoadStatus.Loaded, results[0].Status);
        Assert.Null(results[0].RoutePrefix);
    }

    [Fact]
    public void LoadModules_ReportsErrorForMissingAssembly()
    {
        var loader = new ModuleLoader();
        var repos = new[]
        {
            new RepositoryConfig { Name = "missing-svc", Enabled = true, Type = RepositoryType.Microservice, Path = "/nonexistent/path.dll" }
        };

        var (modules, results) = loader.LoadModules(repos);

        Assert.Empty(modules);
        Assert.Single(results);
        Assert.Equal(ModuleLoadStatus.Error, results[0].Status);
        Assert.Contains("amalgam build", results[0].ErrorMessage!);
    }

    [Fact]
    public void LoadModules_UsesDefaultRoutePrefixFromName()
    {
        var loader = new ModuleLoader();
        var repos = new[]
        {
            new RepositoryConfig { Name = "my-api", Enabled = false, Type = RepositoryType.Microservice }
        };

        var (_, results) = loader.LoadModules(repos);

        Assert.Equal("/my-api", results[0].RoutePrefix);
    }

    [Fact]
    public void LoadModules_UsesConfiguredRoutePrefix()
    {
        var loader = new ModuleLoader();
        var repos = new[]
        {
            new RepositoryConfig { Name = "my-api", Enabled = false, Type = RepositoryType.Microservice, RoutePrefix = "/api/v2" }
        };

        var (_, results) = loader.LoadModules(repos);

        Assert.Equal("/api/v2", results[0].RoutePrefix);
    }

    [Fact]
    public void DiscoverModules_FindsImplementations()
    {
        var types = ModuleLoader.DiscoverModules(typeof(TestModule).Assembly);

        Assert.Contains(types, t => t == typeof(TestModule));
    }
}
