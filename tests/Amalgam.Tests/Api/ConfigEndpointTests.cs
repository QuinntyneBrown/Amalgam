using System.Net;
using System.Net.Http.Json;
using System.Text;
using Amalgam.Core.Configuration;

namespace Amalgam.Tests.Api;

public class ConfigEndpointTests : ApiTestBase
{
    [Fact]
    public async Task GetConfig_ReturnsDeserializedConfig()
    {
        var config = CreateTestConfig();
        WriteConfig(config);

        var response = await Client.GetAsync("/api/config");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<AmalgamConfig>(JsonOptions);
        Assert.NotNull(result);
        Assert.Equal(2, result.Repositories.Count);
    }

    [Fact]
    public async Task GetConfig_WhenNoFile_ReturnsEmptyConfig()
    {
        var response = await Client.GetAsync("/api/config");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<AmalgamConfig>(JsonOptions);
        Assert.NotNull(result);
        Assert.Empty(result.Repositories);
    }

    [Fact]
    public async Task PutConfig_ValidConfig_SavesAndReturns200()
    {
        var config = CreateTestConfig();

        var response = await Client.PutAsJsonAsync("/api/config", config);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify saved to disk
        var saved = ReadConfig();
        Assert.Equal(2, saved.Repositories.Count);
    }

    [Fact]
    public async Task PutConfig_InvalidConfig_Returns400WithErrors()
    {
        var config = new AmalgamConfig
        {
            Repositories = new List<RepositoryConfig>
            {
                new() { Name = "svc", Type = RepositoryType.Microservice, Path = "/nonexistent/path" }
            }
        };

        var response = await Client.PutAsJsonAsync("/api/config", config);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("errors", content);
    }

    [Fact]
    public async Task GetYaml_ReturnsPlainTextYaml()
    {
        var config = CreateTestConfig();
        WriteConfig(config);

        var response = await Client.GetAsync("/api/config/yaml");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var yaml = await response.Content.ReadAsStringAsync();
        Assert.Contains("user-service", yaml);
        Assert.Contains("repositories", yaml);
    }

    [Fact]
    public async Task PutYaml_ValidYaml_SavesAndReturns200()
    {
        var svcDir = Path.Combine(TempDir, "test-svc");
        Directory.CreateDirectory(svcDir);
        File.WriteAllText(Path.Combine(svcDir, "Test.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

        var yaml = $"repositories:\n- name: test-svc\n  type: microservice\n  path: {svcDir.Replace("\\", "/")}\n  enabled: true\n";
        var request = new { yaml };

        var response = await Client.PutAsJsonAsync("/api/config/yaml", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PutYaml_InvalidYaml_Returns400()
    {
        var request = new { yaml = ": invalid: yaml: [" };

        var response = await Client.PutAsJsonAsync("/api/config/yaml", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Validate_ValidConfig_ReturnsIsValidTrue()
    {
        var config = CreateTestConfig();

        var response = await Client.PostAsJsonAsync("/api/config/validate", config);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"isValid\":true", content);
        Assert.Contains("\"errors\":[]", content);
    }

    [Fact]
    public async Task Validate_InvalidConfig_ReturnsIsValidFalseWithErrors()
    {
        var config = new AmalgamConfig
        {
            Repositories = new List<RepositoryConfig>
            {
                new() { Name = "bad", Type = RepositoryType.Microservice, Path = "/nonexistent" }
            }
        };

        var response = await Client.PostAsJsonAsync("/api/config/validate", config);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"isValid\":false", content);
        Assert.Contains("errors", content);
    }
}
