using Amalgam.Core.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Amalgam.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScanController : ControllerBase
{
    [HttpPost]
    public IActionResult Scan([FromBody] ScanRequest request)
    {
        if (!Directory.Exists(request.Path))
            return BadRequest(new { error = $"Directory does not exist: {request.Path}" });

        var config = DirectoryScanner.Scan(request.Path);
        return Ok(config.Repositories);
    }
}

public class ScanRequest
{
    public string Path { get; set; } = string.Empty;
}
