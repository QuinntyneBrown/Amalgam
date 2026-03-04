using System.Net;
using System.Net.Http.Json;
using Amalgam.Api.Services;

namespace Amalgam.Tests.Api;

public class TemplatesEndpointTests : ApiTestBase
{
    [Fact]
    public async Task GetAll_ReturnsAtLeast3Templates()
    {
        var response = await Client.GetAsync("/api/templates");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var templates = await response.Content.ReadFromJsonAsync<List<TemplateSummary>>();
        Assert.NotNull(templates);
        Assert.True(templates.Count >= 3);
    }

    [Fact]
    public async Task GetAll_EachTemplateHasRequiredFields()
    {
        var response = await Client.GetAsync("/api/templates");
        var templates = await response.Content.ReadFromJsonAsync<List<TemplateSummary>>();

        foreach (var template in templates!)
        {
            Assert.NotEmpty(template.Id);
            Assert.NotEmpty(template.Name);
            Assert.NotEmpty(template.Description);
            Assert.True(template.RepositoryCount > 0);
        }
    }

    [Fact]
    public async Task GetById_ExistingTemplate_ReturnsFullConfig()
    {
        var response = await Client.GetAsync("/api/templates/basic-microservice");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var template = await response.Content.ReadFromJsonAsync<TemplateInfo>();
        Assert.NotNull(template);
        Assert.Equal("Basic Microservice Setup", template.Name);
        Assert.NotNull(template.Config);
        Assert.True(template.Config.Repositories.Count > 0);
    }

    [Fact]
    public async Task GetById_UnknownTemplate_Returns404()
    {
        var response = await Client.GetAsync("/api/templates/unknown");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
