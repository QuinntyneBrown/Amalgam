using System.Diagnostics;
using System.Reflection;
using Amalgam.Core.Configuration;
using Amalgam.Core.Modules;
using Amalgam.Host.Diagnostics;

namespace Amalgam.Host;

public class ModuleLoader
{
    public (List<LoadedModule> Modules, List<ModuleLoadResult> Results) LoadModules(
        IEnumerable<RepositoryConfig> repositories)
    {
        var modules = new List<LoadedModule>();
        var results = new List<ModuleLoadResult>();

        foreach (var repo in repositories)
        {
            var sw = Stopwatch.StartNew();
            var routePrefix = repo.RoutePrefix ?? $"/{repo.Name}";

            if (!repo.Enabled)
            {
                sw.Stop();
                results.Add(new ModuleLoadResult
                {
                    ServiceName = repo.Name,
                    Status = ModuleLoadStatus.Skipped,
                    RoutePrefix = routePrefix,
                    LoadTime = sw.Elapsed
                });
                continue;
            }

            if (repo.Type == RepositoryType.Library)
            {
                // Libraries don't have modules — they provide shared services
                results.Add(new ModuleLoadResult
                {
                    ServiceName = repo.Name,
                    Status = ModuleLoadStatus.Loaded,
                    RoutePrefix = null,
                    LoadTime = sw.Elapsed
                });
                continue;
            }

            try
            {
                var assembly = LoadAssembly(repo.Path);
                var moduleTypes = DiscoverModules(assembly);

                if (moduleTypes.Count == 0)
                {
                    sw.Stop();
                    results.Add(new ModuleLoadResult
                    {
                        ServiceName = repo.Name,
                        Status = ModuleLoadStatus.Skipped,
                        RoutePrefix = routePrefix,
                        LoadTime = sw.Elapsed,
                        ErrorMessage = "No IAmalgamModule implementation found in assembly."
                    });
                    continue;
                }

                foreach (var moduleType in moduleTypes)
                {
                    var instance = (IAmalgamModule)Activator.CreateInstance(moduleType)!;
                    modules.Add(new LoadedModule(instance, routePrefix, repo));
                }

                sw.Stop();
                results.Add(new ModuleLoadResult
                {
                    ServiceName = repo.Name,
                    Status = ModuleLoadStatus.Loaded,
                    RoutePrefix = routePrefix,
                    LoadTime = sw.Elapsed
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                results.Add(new ModuleLoadResult
                {
                    ServiceName = repo.Name,
                    Status = ModuleLoadStatus.Error,
                    RoutePrefix = routePrefix,
                    LoadTime = sw.Elapsed,
                    ErrorMessage = ErrorDiagnostics.Diagnose(ex)
                });
            }
        }

        return (modules, results);
    }

    public virtual Assembly LoadAssembly(string path)
    {
        return Assembly.LoadFrom(path);
    }

    public static List<Type> DiscoverModules(Assembly assembly)
    {
        return assembly.GetExportedTypes()
            .Where(t => typeof(IAmalgamModule).IsAssignableFrom(t) && t is { IsAbstract: false, IsInterface: false })
            .ToList();
    }
}

public record LoadedModule(IAmalgamModule Module, string RoutePrefix, RepositoryConfig Config);
