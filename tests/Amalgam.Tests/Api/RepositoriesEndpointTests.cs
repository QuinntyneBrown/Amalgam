using System.Net;
using System.Net.Http.Json;
using Amalgam.Core.Configuration;

namespace Amalgam.Tests.Api;

public class RepositoriesEndpointTests : ApiTestBase
{
    [Fact]
    public async Task GetAll_ReturnsRepositoryList()
    {
        var config = CreateTestConfig();
        WriteConfig(config);

        var response = await Client.GetAsync("/api/repositories");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var repos = await response.Content.ReadFromJsonAsync<List<RepositoryConfig>>();
        Assert.NotNull(repos);
        Assert.Equal(2, repos.Count);
    }

    [Fact]
    public async Task GetByName_ExistingRepo_ReturnsRepo()
    {
        var config = CreateTestConfig();
        WriteConfig(config);

        var response = await Client.GetAsync("/api/repositories/user-service");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var repo = await response.Content.ReadFromJsonAsync<RepositoryConfig>();
        Assert.NotNull(repo);
        Assert.Equal("user-service", repo.Name);
    }

    [Fact]
    public async Task GetByName_NonExistent_Returns404()
    {
        WriteConfig(CreateTestConfig());

        var response = await Client.GetAsync("/api/repositories/nonexistent");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_NewRepo_Returns201AndSaves()
    {
        WriteConfig(CreateTestConfig());

        var newDir = Path.Combine(TempDir, "new-lib");
        Directory.CreateDirectory(newDir);

        var newRepo = new RepositoryConfig
        {
            Name = "new-lib",
            Type = RepositoryType.Library,
            Path = newDir,
            Enabled = true
        };

        var response = await Client.PostAsJsonAsync("/api/repositories", newRepo);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Verify saved
        var saved = ReadConfig();
        Assert.Equal(3, saved.Repositories.Count);
    }

    [Fact]
    public async Task Post_DuplicateName_Returns409()
    {
        WriteConfig(CreateTestConfig());

        var repo = new RepositoryConfig
        {
            Name = "user-service",
            Type = RepositoryType.Microservice,
            Path = "/some/path"
        };

        var response = await Client.PostAsJsonAsync("/api/repositories", repo);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Put_ExistingRepo_Updates()
    {
        var config = CreateTestConfig();
        WriteConfig(config);

        var updatedRepo = config.Repositories[0];
        updatedRepo.Enabled = false;

        var response = await Client.PutAsJsonAsync("/api/repositories/user-service", updatedRepo);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var saved = ReadConfig();
        Assert.False(saved.Repositories[0].Enabled);
    }

    [Fact]
    public async Task Put_NonExistent_Returns404()
    {
        WriteConfig(CreateTestConfig());

        var repo = new RepositoryConfig { Name = "ghost", Type = RepositoryType.Library, Path = "/x" };
        var response = await Client.PutAsJsonAsync("/api/repositories/ghost", repo);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingRepo_RemovesAndReturns204()
    {
        WriteConfig(CreateTestConfig());

        var response = await Client.DeleteAsync("/api/repositories/shared-lib");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var saved = ReadConfig();
        Assert.Single(saved.Repositories);
    }

    [Fact]
    public async Task Delete_NonExistent_Returns404()
    {
        WriteConfig(CreateTestConfig());

        var response = await Client.DeleteAsync("/api/repositories/ghost");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Toggle_FlipsEnabledFlag()
    {
        WriteConfig(CreateTestConfig());

        var response = await Client.PatchAsync("/api/repositories/user-service/toggle", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var repo = await response.Content.ReadFromJsonAsync<RepositoryConfig>();
        Assert.NotNull(repo);
        Assert.False(repo.Enabled); // was true, now false

        // Toggle back
        response = await Client.PatchAsync("/api/repositories/user-service/toggle", null);
        repo = await response.Content.ReadFromJsonAsync<RepositoryConfig>();
        Assert.True(repo!.Enabled);
    }
}
