using System.Diagnostics;
using Amalgam.Core.Configuration;

namespace Amalgam.Core.Plugins;

public class NpmLinkResult
{
    public string PluginName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Error { get; set; }
}

public class NpmLinkService
{
    private readonly IProcessRunner _processRunner;

    public NpmLinkService() : this(new DefaultProcessRunner()) { }

    public NpmLinkService(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    /// <summary>
    /// Links plugin repos into the dashboard via npm link.
    /// </summary>
    public List<NpmLinkResult> LinkPlugins(AmalgamConfig config, string? onlyPlugin = null)
    {
        var results = new List<NpmLinkResult>();
        var dashboardPath = GetDashboardPath(config);
        if (dashboardPath == null) return results;

        var plugins = config.Repositories
            .Where(r => r.Type == RepositoryType.Plugin && r.Enabled)
            .Where(r => onlyPlugin == null || r.Name.Equals(onlyPlugin, StringComparison.OrdinalIgnoreCase));

        foreach (var plugin in plugins)
        {
            var result = new NpmLinkResult { PluginName = plugin.Name };

            try
            {
                var packageJsonPath = Path.Combine(plugin.Path, "package.json");
                var packageName = plugin.PackageName ?? PackageJsonParser.GetPackageName(packageJsonPath);
                if (packageName == null)
                {
                    result.Error = "Could not determine package name from package.json";
                    results.Add(result);
                    continue;
                }

                // Run npm link in the plugin directory
                var linkResult = _processRunner.Run("npm", "link", plugin.Path);
                if (!linkResult.Success)
                {
                    result.Error = $"npm link failed in plugin dir: {linkResult.Error}";
                    results.Add(result);
                    continue;
                }

                // Run npm link <package-name> in the dashboard directory
                var linkInDash = _processRunner.Run("npm", $"link {packageName}", dashboardPath);
                if (!linkInDash.Success)
                {
                    result.Error = $"npm link {packageName} failed in dashboard: {linkInDash.Error}";
                    results.Add(result);
                    continue;
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// Unlinks all plugins from the dashboard and restores node_modules.
    /// </summary>
    public List<NpmLinkResult> UnlinkPlugins(AmalgamConfig config)
    {
        var results = new List<NpmLinkResult>();
        var dashboardPath = GetDashboardPath(config);
        if (dashboardPath == null) return results;

        foreach (var plugin in config.Repositories.Where(r => r.Type == RepositoryType.Plugin))
        {
            var result = new NpmLinkResult { PluginName = plugin.Name };

            try
            {
                var packageJsonPath = Path.Combine(plugin.Path, "package.json");
                var packageName = plugin.PackageName ?? PackageJsonParser.GetPackageName(packageJsonPath);
                if (packageName == null)
                {
                    result.Error = "Could not determine package name";
                    results.Add(result);
                    continue;
                }

                var unlinkResult = _processRunner.Run("npm", $"unlink {packageName}", dashboardPath);
                result.Success = unlinkResult.Success;
                if (!unlinkResult.Success)
                    result.Error = unlinkResult.Error;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            results.Add(result);
        }

        // Restore node_modules
        _processRunner.Run("npm", "install", dashboardPath);

        return results;
    }

    private static string? GetDashboardPath(AmalgamConfig config)
    {
        // Check FrontendConfig.DashboardPath first
        if (!string.IsNullOrEmpty(config.Frontend.DashboardPath))
            return config.Frontend.DashboardPath;

        // Fallback: find the Dashboard repo
        var dashboard = config.Repositories.FirstOrDefault(r => r.Type == RepositoryType.Dashboard);
        return dashboard?.Path;
    }
}

public interface IProcessRunner
{
    ProcessRunResult Run(string command, string arguments, string workingDirectory);
}

public class ProcessRunResult
{
    public bool Success { get; set; }
    public string Output { get; set; } = string.Empty;
    public string? Error { get; set; }
}

public class DefaultProcessRunner : IProcessRunner
{
    public ProcessRunResult Run(string command, string arguments, string workingDirectory)
    {
        var psi = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null)
            return new ProcessRunResult { Error = "Failed to start process" };

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return new ProcessRunResult
        {
            Success = process.ExitCode == 0,
            Output = output,
            Error = process.ExitCode != 0 ? error : null
        };
    }
}
