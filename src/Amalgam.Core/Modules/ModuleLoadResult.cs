namespace Amalgam.Core.Modules;

public enum ModuleLoadStatus
{
    Loaded,
    Skipped,
    Error
}

public class ModuleLoadResult
{
    public string ServiceName { get; set; } = string.Empty;
    public ModuleLoadStatus Status { get; set; }
    public string? RoutePrefix { get; set; }
    public TimeSpan LoadTime { get; set; }
    public string? ErrorMessage { get; set; }
}
