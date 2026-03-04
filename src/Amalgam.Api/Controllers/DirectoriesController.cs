using Microsoft.AspNetCore.Mvc;

namespace Amalgam.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DirectoriesController : ControllerBase
{
    [HttpGet]
    public IActionResult Get([FromQuery] string path, [FromQuery] string? prefix = null)
    {
        if (!Directory.Exists(path))
            return Ok(Array.Empty<string>());

        try
        {
            var dirs = Directory.GetDirectories(path)
                .Select(d => Path.GetFileName(d))
                .Where(name => !name.StartsWith('.'))
                .OrderBy(name => name);

            if (!string.IsNullOrEmpty(prefix))
            {
                dirs = dirs.Where(name => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                           .OrderBy(name => name);
            }

            return Ok(dirs);
        }
        catch (UnauthorizedAccessException)
        {
            return Ok(Array.Empty<string>());
        }
    }
}
