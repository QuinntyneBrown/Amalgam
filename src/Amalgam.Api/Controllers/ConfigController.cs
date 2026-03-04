using Amalgam.Api.Services;
using Amalgam.Core.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Amalgam.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly ConfigFileService _configService;

    public ConfigController(ConfigFileService configService)
    {
        _configService = configService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var config = _configService.Load();
        return Ok(config);
    }

    [HttpPut]
    public IActionResult Put([FromBody] AmalgamConfig config)
    {
        var errors = ConfigValidator.Validate(config);
        if (errors.Count > 0)
            return BadRequest(new { errors });

        _configService.Save(config);
        return Ok(config);
    }

    [HttpGet("yaml")]
    public IActionResult GetYaml()
    {
        var yaml = _configService.LoadYaml();
        return Content(yaml, "text/plain");
    }

    [HttpPut("yaml")]
    public IActionResult PutYaml([FromBody] YamlRequest request)
    {
        AmalgamConfig config;
        try
        {
            config = ConfigLoader.Parse(request.Yaml);
        }
        catch (Exception ex)
        {
            return BadRequest(new { errors = new[] { $"Invalid YAML: {ex.Message}" } });
        }

        var errors = ConfigValidator.Validate(config);
        if (errors.Count > 0)
            return BadRequest(new { errors });

        _configService.SaveYaml(request.Yaml);
        return Ok(config);
    }

    [HttpPost("validate")]
    public IActionResult Validate([FromBody] AmalgamConfig? config = null)
    {
        config ??= _configService.Load();
        var errors = ConfigValidator.Validate(config);
        return Ok(new ValidationResult { IsValid = errors.Count == 0, Errors = errors });
    }
}

public class YamlRequest
{
    public string Yaml { get; set; } = string.Empty;
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
