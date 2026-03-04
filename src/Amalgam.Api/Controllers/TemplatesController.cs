using Amalgam.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Amalgam.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly TemplateService _templateService;

    public TemplatesController(TemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_templateService.GetAll());
    }

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var template = _templateService.GetById(id);
        if (template == null)
            return NotFound(new { error = $"Template '{id}' not found" });

        return Ok(template);
    }
}
