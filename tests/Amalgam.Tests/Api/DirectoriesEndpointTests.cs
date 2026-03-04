using System.Net;
using System.Net.Http.Json;

namespace Amalgam.Tests.Api;

public class DirectoriesEndpointTests : ApiTestBase
{
    [Fact]
    public async Task Get_ExistingPath_ReturnsSubdirectories()
    {
        // Create subdirectories
        Directory.CreateDirectory(Path.Combine(TempDir, "alpha"));
        Directory.CreateDirectory(Path.Combine(TempDir, "beta"));
        Directory.CreateDirectory(Path.Combine(TempDir, "gamma"));

        var response = await Client.GetAsync($"/api/directories?path={Uri.EscapeDataString(TempDir)}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dirs = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(dirs);
        Assert.Contains("alpha", dirs);
        Assert.Contains("beta", dirs);
        Assert.Contains("gamma", dirs);
    }

    [Fact]
    public async Task Get_WithPrefix_FiltersResults()
    {
        Directory.CreateDirectory(Path.Combine(TempDir, "alpha"));
        Directory.CreateDirectory(Path.Combine(TempDir, "beta"));

        var response = await Client.GetAsync($"/api/directories?path={Uri.EscapeDataString(TempDir)}&prefix=al");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dirs = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(dirs);
        Assert.Single(dirs);
        Assert.Equal("alpha", dirs[0]);
    }

    [Fact]
    public async Task Get_NonExistentPath_ReturnsEmptyArray()
    {
        var response = await Client.GetAsync("/api/directories?path=/nonexistent/path");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dirs = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(dirs);
        Assert.Empty(dirs);
    }

    [Fact]
    public async Task Get_HiddenDirectories_AreExcluded()
    {
        Directory.CreateDirectory(Path.Combine(TempDir, ".hidden"));
        Directory.CreateDirectory(Path.Combine(TempDir, "visible"));

        var response = await Client.GetAsync($"/api/directories?path={Uri.EscapeDataString(TempDir)}");
        var dirs = await response.Content.ReadFromJsonAsync<List<string>>();

        Assert.DoesNotContain(".hidden", dirs!);
        Assert.Contains("visible", dirs);
    }
}
