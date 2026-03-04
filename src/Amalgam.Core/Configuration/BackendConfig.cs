namespace Amalgam.Core.Configuration;

public class BackendConfig
{
    public int Port { get; set; } = 5000;
    public Dictionary<string, string> Environment { get; set; } = new();
}
