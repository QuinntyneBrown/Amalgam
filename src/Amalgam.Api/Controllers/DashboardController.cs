using Amalgam.Api.Services;
using Amalgam.Core.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Amalgam.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ConfigFileService _configService;

    public DashboardController(ConfigFileService configService)
    {
        _configService = configService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var config = _configService.Load();
        var errors = ConfigValidator.Validate(config);

        var countByType = config.Repositories
            .GroupBy(r => r.Type)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        // Ensure all types are present
        foreach (var type in Enum.GetNames<RepositoryType>())
        {
            countByType.TryAdd(type, 0);
        }

        return Ok(new
        {
            totalRepositories = config.Repositories.Count,
            countByType,
            validation = new
            {
                isValid = errors.Count == 0,
                errorCount = errors.Count
            }
        });
    }
}
