namespace Amalgam.Core.Configuration;

public class AmalgamConfig
{
    public List<RepositoryConfig> Repositories { get; set; } = new();
    public BackendConfig Backend { get; set; } = new();
    public FrontendConfig Frontend { get; set; } = new();
}
