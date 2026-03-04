using Amalgam.Core.Configuration;
using Amalgam.Core.Plugins;

namespace Amalgam.Tests.Plugins;

public class NpmLinkServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly MockProcessRunner _mockRunner;
    private readonly NpmLinkService _service;

    public NpmLinkServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "amalgam-npm-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        _mockRunner = new MockProcessRunner();
        _service = new NpmLinkService(_mockRunner);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void LinkPlugins_RunsNpmLinkCommands()
    {
        var pluginDir = CreatePluginDir("my-plugin", "@acme/my-plugin");
        var dashboardDir = CreateDir("dashboard");

        var config = MakeConfig(pluginDir, dashboardDir);

        var results = _service.LinkPlugins(config);

        Assert.Single(results);
        Assert.True(results[0].Success);
        Assert.Equal("my-plugin", results[0].PluginName);

        // Should have run: npm link (in plugin dir), npm link @acme/my-plugin (in dashboard)
        Assert.Equal(2, _mockRunner.Commands.Count);
        Assert.Equal(("npm", "link", pluginDir), _mockRunner.Commands[0]);
        Assert.Equal(("npm", "link @acme/my-plugin", dashboardDir), _mockRunner.Commands[1]);
    }

    [Fact]
    public void LinkPlugins_OnlyPlugin_FiltersCorrectly()
    {
        var plugin1Dir = CreatePluginDir("plugin-a", "@acme/plugin-a");
        var plugin2Dir = CreatePluginDir("plugin-b", "@acme/plugin-b");
        var dashboardDir = CreateDir("dashboard");

        var config = new AmalgamConfig
        {
            Repositories = new List<RepositoryConfig>
            {
                new() { Name = "plugin-a", Type = RepositoryType.Plugin, Path = plugin1Dir },
                new() { Name = "plugin-b", Type = RepositoryType.Plugin, Path = plugin2Dir },
                new() { Name = "dashboard", Type = RepositoryType.Dashboard, Path = dashboardDir }
            }
        };

        var results = _service.LinkPlugins(config, onlyPlugin: "plugin-b");

        Assert.Single(results);
        Assert.Equal("plugin-b", results[0].PluginName);
    }

    [Fact]
    public void UnlinkPlugins_RunsUnlinkAndInstall()
    {
        var pluginDir = CreatePluginDir("my-plugin", "@acme/my-plugin");
        var dashboardDir = CreateDir("dashboard");

        var config = MakeConfig(pluginDir, dashboardDir);

        var results = _service.UnlinkPlugins(config);

        Assert.Single(results);
        Assert.True(results[0].Success);

        // Should have: npm unlink @acme/my-plugin (in dashboard), then npm install (in dashboard)
        Assert.Equal(2, _mockRunner.Commands.Count);
        Assert.Equal(("npm", "unlink @acme/my-plugin", dashboardDir), _mockRunner.Commands[0]);
        Assert.Equal(("npm", "install", dashboardDir), _mockRunner.Commands[1]);
    }

    [Fact]
    public void LinkPlugins_ReportsError_WhenNpmFails()
    {
        var pluginDir = CreatePluginDir("bad-plugin", "@acme/bad");
        var dashboardDir = CreateDir("dashboard");

        _mockRunner.FailNextCommand = true;
        var config = MakeConfig(pluginDir, dashboardDir);

        var results = _service.LinkPlugins(config);

        Assert.Single(results);
        Assert.False(results[0].Success);
        Assert.Contains("npm link failed", results[0].Error);
    }

    private AmalgamConfig MakeConfig(string pluginDir, string dashboardDir)
    {
        return new AmalgamConfig
        {
            Repositories = new List<RepositoryConfig>
            {
                new() { Name = "my-plugin", Type = RepositoryType.Plugin, Path = pluginDir },
                new() { Name = "dashboard", Type = RepositoryType.Dashboard, Path = dashboardDir }
            }
        };
    }

    private string CreatePluginDir(string name, string packageName)
    {
        var dir = CreateDir(name);
        File.WriteAllText(Path.Combine(dir, "package.json"),
            $$"""{ "name": "{{packageName}}", "version": "1.0.0" }""");
        return dir;
    }

    private string CreateDir(string name)
    {
        var dir = Path.Combine(_tempDir, name);
        Directory.CreateDirectory(dir);
        return dir;
    }
}

public class MockProcessRunner : IProcessRunner
{
    public List<(string Command, string Args, string WorkDir)> Commands { get; } = new();
    public bool FailNextCommand { get; set; }

    public ProcessRunResult Run(string command, string arguments, string workingDirectory)
    {
        Commands.Add((command, arguments, workingDirectory));

        if (FailNextCommand)
        {
            FailNextCommand = false;
            return new ProcessRunResult { Success = false, Error = "mock failure" };
        }

        return new ProcessRunResult { Success = true, Output = "ok" };
    }
}
