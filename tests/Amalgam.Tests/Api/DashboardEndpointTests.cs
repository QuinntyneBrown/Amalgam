using System.Net;
using System.Text.Json;

namespace Amalgam.Tests.Api;

public class DashboardEndpointTests : ApiTestBase
{
    [Fact]
    public async Task Get_ReturnsRepositoryCountsAndValidation()
    {
        WriteConfig(CreateTestConfig());

        var response = await Client.GetAsync("/api/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        var root = json.RootElement;

        Assert.Equal(2, root.GetProperty("totalRepositories").GetInt32());

        var countByType = root.GetProperty("countByType");
        Assert.Equal(1, countByType.GetProperty("Microservice").GetInt32());
        Assert.Equal(1, countByType.GetProperty("Library").GetInt32());
        Assert.Equal(0, countByType.GetProperty("Plugin").GetInt32());
        Assert.Equal(0, countByType.GetProperty("Dashboard").GetInt32());

        var validation = root.GetProperty("validation");
        Assert.True(validation.GetProperty("isValid").GetBoolean());
        Assert.Equal(0, validation.GetProperty("errorCount").GetInt32());
    }

    [Fact]
    public async Task Get_EmptyConfig_ReturnsZeroCounts()
    {
        var response = await Client.GetAsync("/api/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        Assert.Equal(0, json.RootElement.GetProperty("totalRepositories").GetInt32());
    }
}
