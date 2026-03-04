using Amalgam.Core.Configuration;

namespace Amalgam.Core.PackageResolution;

public class NuGetOverrideService
{
    private const string AmalgamDir = ".amalgam";
    private const string PropsFileName = "Directory.Build.props";

    /// <summary>
    /// For each microservice, generates a Directory.Build.props that replaces
    /// matching PackageReferences with ProjectReferences to local library sources.
    /// </summary>
    public List<string> GenerateOverrides(AmalgamConfig config)
    {
        var generatedFiles = new List<string>();

        // 1. Collect library info: map NuGet package name -> csproj path
        var libraries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var repo in config.Repositories.Where(r => r.Type == RepositoryType.Library && r.Enabled))
        {
            var csprojPath = FindCsproj(repo.Path);
            if (csprojPath == null) continue;

            var info = CsprojParser.Parse(csprojPath);
            var packageName = info.GetPackageName() ?? System.IO.Path.GetFileNameWithoutExtension(csprojPath);
            libraries[packageName] = csprojPath;
        }

        if (libraries.Count == 0) return generatedFiles;

        // 2. For each microservice, find matching PackageReferences and generate props
        foreach (var repo in config.Repositories.Where(r => r.Type == RepositoryType.Microservice && r.Enabled))
        {
            var csprojPath = FindCsproj(repo.Path);
            if (csprojPath == null) continue;

            var info = CsprojParser.Parse(csprojPath);
            var matches = new List<(string PackageName, string LibraryCsprojPath)>();

            foreach (var pkgRef in info.PackageReferences)
            {
                if (libraries.TryGetValue(pkgRef.Name, out var libCsprojPath))
                {
                    matches.Add((pkgRef.Name, libCsprojPath));
                }
            }

            if (matches.Count == 0) continue;

            // Generate .amalgam/Directory.Build.props
            var amalgamDir = Path.Combine(repo.Path, AmalgamDir);
            Directory.CreateDirectory(amalgamDir);

            var amalgamPropsPath = Path.Combine(amalgamDir, PropsFileName);
            var content = GeneratePropsContent(matches, repo.Path);
            File.WriteAllText(amalgamPropsPath, content);
            generatedFiles.Add(amalgamPropsPath);

            // Generate or update root Directory.Build.props to import the .amalgam one
            var rootPropsPath = Path.Combine(repo.Path, PropsFileName);
            EnsureRootPropsImport(rootPropsPath);
            generatedFiles.Add(rootPropsPath);
        }

        return generatedFiles;
    }

    /// <summary>
    /// Removes all generated .amalgam directories from microservice repos.
    /// </summary>
    public void CleanOverrides(AmalgamConfig config)
    {
        foreach (var repo in config.Repositories.Where(r => r.Type == RepositoryType.Microservice))
        {
            var amalgamDir = Path.Combine(repo.Path, AmalgamDir);
            if (Directory.Exists(amalgamDir))
            {
                Directory.Delete(amalgamDir, recursive: true);
            }
        }
    }

    private static string GeneratePropsContent(
        List<(string PackageName, string LibraryCsprojPath)> matches,
        string microservicePath)
    {
        var removeItems = string.Join(Environment.NewLine, matches.Select(m =>
            $"    <PackageReference Remove=\"{m.PackageName}\" />"));

        var addItems = string.Join(Environment.NewLine, matches.Select(m =>
        {
            var relativePath = Path.GetRelativePath(microservicePath, m.LibraryCsprojPath);
            return $"    <ProjectReference Include=\"{relativePath}\" />";
        }));

        return $"""
            <Project>
              <ItemGroup>
            {removeItems}
              </ItemGroup>
              <ItemGroup>
            {addItems}
              </ItemGroup>
            </Project>
            """;
    }

    private static void EnsureRootPropsImport(string rootPropsPath)
    {
        var importLine = $"  <Import Project=\"{AmalgamDir}/{PropsFileName}\" Condition=\"Exists('{AmalgamDir}/{PropsFileName}')\" />";

        if (File.Exists(rootPropsPath))
        {
            var existing = File.ReadAllText(rootPropsPath);
            if (existing.Contains($"{AmalgamDir}/{PropsFileName}"))
                return; // already has the import
        }

        // Create a minimal Directory.Build.props that imports the .amalgam one
        var content = $"""
            <Project>
            {importLine}
            </Project>
            """;
        File.WriteAllText(rootPropsPath, content);
    }

    private static string? FindCsproj(string repoPath)
    {
        if (!Directory.Exists(repoPath)) return null;

        var files = Directory.GetFiles(repoPath, "*.csproj", SearchOption.AllDirectories);
        return files.Length > 0 ? files[0] : null;
    }
}
