using Amalgam.Api.Services;
using Amalgam.Core.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Amalgam.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RepositoriesController : ControllerBase
{
    private readonly ConfigFileService _configService;

    public RepositoriesController(ConfigFileService configService)
    {
        _configService = configService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var config = _configService.Load();
        return Ok(config.Repositories);
    }

    [HttpGet("{name}")]
    public IActionResult GetByName(string name)
    {
        var config = _configService.Load();
        var repo = config.Repositories.FirstOrDefault(r =>
            string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));

        if (repo == null)
            return NotFound(new { error = $"Repository '{name}' not found" });

        return Ok(repo);
    }

    [HttpPost]
    public IActionResult Create([FromBody] RepositoryConfig repo)
    {
        var config = _configService.Load();

        if (config.Repositories.Any(r =>
            string.Equals(r.Name, repo.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return Conflict(new { error = $"Repository '{repo.Name}' already exists" });
        }

        config.Repositories.Add(repo);
        _configService.Save(config);
        return CreatedAtAction(nameof(GetByName), new { name = repo.Name }, repo);
    }

    [HttpPut("{name}")]
    public IActionResult Update(string name, [FromBody] RepositoryConfig repo)
    {
        var config = _configService.Load();
        var index = config.Repositories.FindIndex(r =>
            string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));

        if (index < 0)
            return NotFound(new { error = $"Repository '{name}' not found" });

        // If renaming, check for conflicts
        if (!string.Equals(repo.Name, name, StringComparison.OrdinalIgnoreCase) &&
            config.Repositories.Any(r => string.Equals(r.Name, repo.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return Conflict(new { error = $"Repository '{repo.Name}' already exists" });
        }

        config.Repositories[index] = repo;
        _configService.Save(config);
        return Ok(repo);
    }

    [HttpDelete("{name}")]
    public IActionResult Delete(string name)
    {
        var config = _configService.Load();
        var removed = config.Repositories.RemoveAll(r =>
            string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));

        if (removed == 0)
            return NotFound(new { error = $"Repository '{name}' not found" });

        _configService.Save(config);
        return NoContent();
    }

    [HttpPatch("{name}/toggle")]
    public IActionResult Toggle(string name)
    {
        var config = _configService.Load();
        var repo = config.Repositories.FirstOrDefault(r =>
            string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));

        if (repo == null)
            return NotFound(new { error = $"Repository '{name}' not found" });

        repo.Enabled = !repo.Enabled;
        _configService.Save(config);
        return Ok(repo);
    }
}
