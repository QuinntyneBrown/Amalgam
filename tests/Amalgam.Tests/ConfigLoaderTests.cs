using Amalgam.Core.Configuration;

namespace Amalgam.Tests;

public class ConfigLoaderTests
{
    [Fact]
    public void Parse_ValidYaml_ReturnsConfig()
    {
        var yaml = @"
repositories:
  - name: my-service
    type: Microservice
    path: /some/path
    enabled: true
backend:
  port: 8080
frontend:
  dashboardPath: /dash
  port: 4000
";
        var config = ConfigLoader.Parse(yaml);

        Assert.Single(config.Repositories);
        Assert.Equal("my-service", config.Repositories[0].Name);
        Assert.Equal(RepositoryType.Microservice, config.Repositories[0].Type);
        Assert.Equal("/some/path", config.Repositories[0].Path);
        Assert.True(config.Repositories[0].Enabled);
        Assert.Equal(8080, config.Backend.Port);
        Assert.Equal("/dash", config.Frontend.DashboardPath);
        Assert.Equal(4000, config.Frontend.Port);
    }

    [Fact]
    public void Parse_MinimalYaml_AppliesDefaults()
    {
        var yaml = @"
repositories: []
";
        var config = ConfigLoader.Parse(yaml);

        Assert.Empty(config.Repositories);
        Assert.Equal(5000, config.Backend.Port);
        Assert.Equal(3000, config.Frontend.Port);
    }

    [Fact]
    public void Parse_EmptyYaml_ReturnsDefaultConfig()
    {
        var config = ConfigLoader.Parse("");

        Assert.NotNull(config);
        Assert.Empty(config.Repositories);
        Assert.Equal(5000, config.Backend.Port);
    }

    [Fact]
    public void Load_MissingFile_ThrowsException()
    {
        Assert.ThrowsAny<IOException>(() =>
            ConfigLoader.Load("/nonexistent/amalgam.yml"));
    }

    [Fact]
    public void Serialize_RoundTrips()
    {
        var config = new AmalgamConfig
        {
            Repositories = new List<RepositoryConfig>
            {
                new() { Name = "svc", Type = RepositoryType.Microservice, Path = "/p" }
            },
            Backend = new BackendConfig { Port = 9000 }
        };

        var yaml = ConfigLoader.Serialize(config);
        var loaded = ConfigLoader.Parse(yaml);

        Assert.Single(loaded.Repositories);
        Assert.Equal("svc", loaded.Repositories[0].Name);
        Assert.Equal(9000, loaded.Backend.Port);
    }
}
