using Backend.Models.System;
using Backend.Services.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Text.Json;

namespace Backend.Controllers.System;

public class MenuController(MenuService _service) : ControllerBase
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    [HttpGet]
    [Authorize("Read")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Read(MenuSearch search, [FromQuery] int page = 1, [FromQuery] int pageSize = 1000)
    {
        var data = await _service.Read(search, page, pageSize);
        
        return Ok(data);
    }
}
