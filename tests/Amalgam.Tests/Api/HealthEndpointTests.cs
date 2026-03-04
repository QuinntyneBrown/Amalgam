using System.Net;

namespace Amalgam.Tests.Api;

public class HealthEndpointTests : ApiTestBase
{
    [Fact]
    public async Task Get_Health_Returns200()
    {
        var response = await Client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_Health_ReturnsHealthyStatus()
    {
        var response = await Client.GetAsync("/api/health");
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("healthy", content);
    }

    [Fact]
    public async Task Json_Responses_UseCamelCase()
    {
        var response = await Client.GetAsync("/api/health");
        var content = await response.Content.ReadAsStringAsync();
        // camelCase: "status" not "Status"
        Assert.Contains("\"status\"", content);
    }
}
