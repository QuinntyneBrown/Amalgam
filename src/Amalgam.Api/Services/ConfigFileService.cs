using Amalgam.Core.Configuration;

namespace Amalgam.Api.Services;

public class ConfigFileService
{
    private readonly string _configPath;
    private readonly object _lock = new();

    public ConfigFileService(IConfiguration configuration)
    {
        _configPath = configuration.GetValue<string>("ConfigPath") ?? "amalgam.yml";
    }

    public ConfigFileService(string configPath)
    {
        _configPath = configPath;
    }

    public string ConfigPath => _configPath;

    public AmalgamConfig Load()
    {
        lock (_lock)
        {
            if (!File.Exists(_configPath))
                return new AmalgamConfig();
            return ConfigLoader.Load(_configPath);
        }
    }

    public void Save(AmalgamConfig config)
    {
        lock (_lock)
        {
            ConfigLoader.Save(config, _configPath);
        }
    }

    public string LoadYaml()
    {
        lock (_lock)
        {
            if (!File.Exists(_configPath))
                return ConfigLoader.Serialize(new AmalgamConfig());
            return File.ReadAllText(_configPath);
        }
    }

    public void SaveYaml(string yaml)
    {
        lock (_lock)
        {
            File.WriteAllText(_configPath, yaml);
        }
    }
}
