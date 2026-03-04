namespace Amalgam.Core.Configuration;

public class ConfigValidator
{
    public static List<string> Validate(AmalgamConfig config)
    {
        var errors = new List<string>();

        ValidatePaths(config, errors);
        ValidateNoDuplicateNames(config, errors);
        ValidateAtMostOneDashboard(config, errors);
        ValidateMicroservicesHaveCsproj(config, errors);
        ValidateMergeConfigs(config, errors);

        return errors;
    }

    private static void ValidatePaths(AmalgamConfig config, List<string> errors)
    {
        foreach (var repo in config.Repositories)
        {
            if (!Directory.Exists(repo.Path))
            {
                errors.Add($"Repository '{repo.Name}': path does not exist: {repo.Path}");
            }
        }
    }

    private static void ValidateNoDuplicateNames(AmalgamConfig config, List<string> errors)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var repo in config.Repositories)
        {
            if (!seen.Add(repo.Name))
            {
                errors.Add($"Duplicate repository name: '{repo.Name}'");
            }
        }
    }

    private static void ValidateAtMostOneDashboard(AmalgamConfig config, List<string> errors)
    {
        var dashboards = config.Repositories
            .Where(r => r.Type == RepositoryType.Dashboard)
            .ToList();

        if (dashboards.Count > 1)
        {
            var names = string.Join(", ", dashboards.Select(d => d.Name));
            errors.Add($"At most one dashboard allowed, found {dashboards.Count}: {names}");
        }
    }

    private static void ValidateMicroservicesHaveCsproj(AmalgamConfig config, List<string> errors)
    {
        foreach (var repo in config.Repositories.Where(r => r.Type == RepositoryType.Microservice))
        {
            if (!Directory.Exists(repo.Path))
                continue; // already reported by path validation

            var csprojFiles = Directory.GetFiles(repo.Path, "*.csproj", SearchOption.TopDirectoryOnly);
            if (csprojFiles.Length == 0)
            {
                errors.Add($"Microservice '{repo.Name}': no .csproj file found in {repo.Path}");
            }
        }
    }

    private static void ValidateMergeConfigs(AmalgamConfig config, List<string> errors)
    {
        foreach (var repo in config.Repositories.Where(r => r.Merge != null))
        {
            var merge = repo.Merge!;

            if (merge.Sources.Count == 0)
            {
                errors.Add($"Repository '{repo.Name}': merge config has no sources");
                continue;
            }

            foreach (var source in merge.Sources)
            {
                var sourcePath = Path.Combine(repo.Path, source);
                if (!Directory.Exists(sourcePath))
                {
                    errors.Add($"Repository '{repo.Name}': merge source directory does not exist: {sourcePath}");
                }
            }

            if (merge.Target != null)
            {
                var targetFound = false;
                foreach (var source in merge.Sources)
                {
                    var sourcePath = Path.GetFullPath(Path.Combine(repo.Path, source));
                    var targetPath = Path.GetFullPath(Path.Combine(repo.Path, merge.Target));
                    if (File.Exists(targetPath) && targetPath.StartsWith(sourcePath, StringComparison.OrdinalIgnoreCase))
                    {
                        targetFound = true;
                        break;
                    }
                }

                if (!targetFound)
                {
                    errors.Add($"Repository '{repo.Name}': merge target '{merge.Target}' not found within any source");
                }
            }
        }
    }
}
