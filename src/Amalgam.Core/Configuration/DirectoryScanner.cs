namespace Amalgam.Core.Configuration;

public class DirectoryScanner
{
    public static AmalgamConfig Scan(string rootDirectory)
    {
        var config = new AmalgamConfig();
        var rootDir = new DirectoryInfo(rootDirectory);

        if (!rootDir.Exists)
            return config;

        foreach (var subDir in rootDir.GetDirectories())
        {
            // Skip hidden directories
            if (subDir.Name.StartsWith('.'))
                continue;

            // Check if it's a git repo
            var gitDir = Path.Combine(subDir.FullName, ".git");
            if (!Directory.Exists(gitDir))
                continue;

            var repo = DetectRepository(subDir);
            if (repo != null)
                config.Repositories.Add(repo);
        }

        return config;
    }

    private static RepositoryConfig? DetectRepository(DirectoryInfo dir)
    {
        var hasCsproj = dir.GetFiles("*.csproj", SearchOption.TopDirectoryOnly).Length > 0;
        var hasPackageJson = File.Exists(Path.Combine(dir.FullName, "package.json"));

        RepositoryType type;
        if (hasCsproj)
        {
            // Check if it looks like a library (has no Program.cs or similar entry point indicators)
            var hasProgramCs = File.Exists(Path.Combine(dir.FullName, "Program.cs"));
            type = hasProgramCs ? RepositoryType.Microservice : RepositoryType.Library;
        }
        else if (hasPackageJson)
        {
            // Check package.json for dashboard indicators
            var packageJsonContent = File.ReadAllText(Path.Combine(dir.FullName, "package.json"));
            type = packageJsonContent.Contains("\"react\"") || packageJsonContent.Contains("\"vue\"") ||
                   packageJsonContent.Contains("\"angular\"") || packageJsonContent.Contains("\"next\"") ||
                   packageJsonContent.Contains("\"vite\"")
                ? RepositoryType.Dashboard
                : RepositoryType.Plugin;
        }
        else
        {
            return null; // Can't determine type
        }

        return new RepositoryConfig
        {
            Name = dir.Name,
            Type = type,
            Path = dir.FullName,
            Enabled = true
        };
    }
}
