using Amalgam.Core.Configuration;
using Amalgam.Core.Modules;

namespace Amalgam.Host;

public class AmalgamHostBuilder
{
    private readonly AmalgamConfig _config;
    private readonly ModuleLoader _loader;

    public AmalgamHostBuilder(AmalgamConfig config, ModuleLoader? loader = null)
    {
        _config = config;
        _loader = loader ?? new ModuleLoader();
    }

    public AmalgamHostBuildResult Build(string[] args)
    {
        var (loadedModules, results) = _loader.LoadModules(_config.Repositories);

        var builder = WebApplication.CreateBuilder(args);

        // Register shared library services first
        foreach (var loaded in loadedModules)
        {
            loaded.Module.ConfigureServices(builder.Services);
        }

        var app = builder.Build();

        // Map each module under its route prefix
        foreach (var loaded in loadedModules)
        {
            var prefix = loaded.RoutePrefix.TrimStart('/');
            app.Map($"/{prefix}", subApp =>
            {
                loaded.Module.ConfigureApp(subApp);
            });
        }

        return new AmalgamHostBuildResult(app, loadedModules, results);
    }
}

public record AmalgamHostBuildResult(
    WebApplication App,
    List<LoadedModule> Modules,
    List<ModuleLoadResult> Results);
