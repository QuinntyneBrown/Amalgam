namespace Amalgam.Core.Configuration;

public class RepositoryConfig
{
    public string Name { get; set; } = string.Empty;
    public RepositoryType Type { get; set; }
    public string Path { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public string? RoutePrefix { get; set; }
    public string? PackageName { get; set; }
    public MergeConfig? Merge { get; set; }
}
