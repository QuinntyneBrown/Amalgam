using System.Net;
using System.Net.Http.Json;
using Amalgam.Core.Configuration;

namespace Amalgam.Tests.Api;

public class ScanEndpointTests : ApiTestBase
{
    [Fact]
    public async Task Scan_ValidDirectory_ReturnsDiscoveredRepos()
    {
        // Create a fake repo directory with git and csproj
        var scanDir = Path.Combine(TempDir, "scan-root");
        Directory.CreateDirectory(scanDir);

        var repoDir = Path.Combine(scanDir, "my-service");
        Directory.CreateDirectory(repoDir);
        Directory.CreateDirectory(Path.Combine(repoDir, ".git"));
        File.WriteAllText(Path.Combine(repoDir, "MyService.csproj"), "<Project/>");
        File.WriteAllText(Path.Combine(repoDir, "Program.cs"), "// entry");

        var response = await Client.PostAsJsonAsync("/api/scan", new { path = scanDir });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var repos = await response.Content.ReadFromJsonAsync<List<RepositoryConfig>>();
        Assert.NotNull(repos);
        Assert.Single(repos);
        Assert.Equal("my-service", repos[0].Name);
        Assert.Equal(RepositoryType.Microservice, repos[0].Type);
    }

    [Fact]
    public async Task Scan_NonExistentPath_Returns400()
    {
        var response = await Client.PostAsJsonAsync("/api/scan", new { path = "/nonexistent/dir" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
