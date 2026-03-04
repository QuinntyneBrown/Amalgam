using Amalgam.Core.Configuration;
using Amalgam.Host;

namespace Amalgam.Tests;

public class AmalgamHostBuilderTests
{
    [Fact]
    public void Build_ReturnsResultWithApp()
    {
        var config = new AmalgamConfig();
        var builder = new AmalgamHostBuilder(config);

        var result = builder.Build(Array.Empty<string>());

        Assert.NotNull(result.App);
        Assert.Empty(result.Modules);
        Assert.Empty(result.Results);
    }

    [Fact]
    public void Build_SkipsDisabledModules()
    {
        var config = new AmalgamConfig
        {
            Repositories = new List<RepositoryConfig>
            {
                new() { Name = "svc-a", Enabled = false, Type = RepositoryType.Microservice }
            }
        };
        var builder = new AmalgamHostBuilder(config);

        var result = builder.Build(Array.Empty<string>());

        Assert.Empty(result.Modules);
        Assert.Single(result.Results);
        Assert.Equal(Core.Modules.ModuleLoadStatus.Skipped, result.Results[0].Status);
    }

    [Fact]
    public void Build_HandlesEmptyRepositoryList()
    {
        var config = new AmalgamConfig { Repositories = new List<RepositoryConfig>() };
        var builder = new AmalgamHostBuilder(config);

        var result = builder.Build(Array.Empty<string>());

        Assert.NotNull(result.App);
        Assert.Empty(result.Modules);
        Assert.Empty(result.Results);
    }
}
