using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amalgam.Api.Services;
using Amalgam.Core.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Amalgam.Tests.Api;

public class ApiTestBase : IDisposable
{
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly string TempDir;
    protected readonly string ConfigPath;
    protected readonly HttpClient Client;
    private readonly WebApplicationFactory<Program> _factory;

    public ApiTestBase()
    {
        TempDir = Path.Combine(Path.GetTempPath(), "amalgam-api-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(TempDir);
        ConfigPath = Path.Combine(TempDir, "amalgam.yml");

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace ConfigFileService with one pointing to our temp file
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ConfigFileService));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddSingleton(new ConfigFileService(ConfigPath));
                });
            });

        Client = _factory.CreateClient();
    }

    protected void WriteConfig(AmalgamConfig config)
    {
        ConfigLoader.Save(config, ConfigPath);
    }

    protected AmalgamConfig ReadConfig()
    {
        return ConfigLoader.Load(ConfigPath);
    }

    protected AmalgamConfig CreateTestConfig()
    {
        // Create actual directories so validation passes
        var svcDir = Path.Combine(TempDir, "user-service");
        Directory.CreateDirectory(svcDir);
        File.WriteAllText(Path.Combine(svcDir, "UserService.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

        var libDir = Path.Combine(TempDir, "shared-lib");
        Directory.CreateDirectory(libDir);

        return new AmalgamConfig
        {
            Repositories = new List<RepositoryConfig>
            {
                new() { Name = "user-service", Type = RepositoryType.Microservice, Path = svcDir, Enabled = true },
                new() { Name = "shared-lib", Type = RepositoryType.Library, Path = libDir, Enabled = true }
            },
            Backend = new BackendConfig { Port = 5000 },
            Frontend = new FrontendConfig { Port = 3000 }
        };
    }

    public void Dispose()
    {
        Client.Dispose();
        _factory.Dispose();
        if (Directory.Exists(TempDir))
            Directory.Delete(TempDir, true);
    }
}
