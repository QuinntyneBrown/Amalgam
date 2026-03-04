namespace Amalgam.Core.Configuration;

public class MergeConfig
{
    public List<string> Sources { get; set; } = new();
    public string? Target { get; set; }
}
