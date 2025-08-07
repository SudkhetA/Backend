using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Text.Json;
using Backend.Models.System;
using Backend.Services.System;
using Backend.Utilities.Helper;

namespace Backend.Controllers.System;

[Route("api/system/[controller]")]
[ApiController]
public class NotificationController(ILogger<NotificationController> _logger, JwtHelper _jwtHelper, NotificationService _service) : ControllerBase
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    [HttpGet]
    [Authorize("Read")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Read(NotificationSearch search, [FromQuery] int page = 1, [FromQuery] int pageSize = 1000)
    {
        var data = await _service.Read(search, page, pageSize);
        
        return Ok(data);
    }

    [HttpPost]
    [Authorize("Create")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromHeader] string authorization, [FromHeader] string userAgent, [FromBody] JsonElement json)
    {
        try
        {
            _service.UserId = _jwtHelper.GetUserId(authorization);
            _service.RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            _service.UserAgent = userAgent;

            var data = json.Deserialize<List<Notification>>(_jsonOptions);
            if (data != null && data.Count != 0) 
            {
                var result = await _service.Create(data);
                _logger.LogInformation("{result} rows were created", result.Count);
                if (result.Count == 0) return Created();

                var url = "api/system/notification?" + string.Join("&", result.Select(x => $"id={x.Id}"));
                return Created(url, result);
            }
            else
            {
                return BadRequest("data not found");
            }
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{message}", ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch]
    [Authorize("Update")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromHeader] string authorization, [FromHeader] string userAgent, [FromBody] JsonElement json)
    {
        try
        {
            _service.UserId = _jwtHelper.GetUserId(authorization);
            _service.RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            _service.UserAgent = userAgent;

            var data = json.Deserialize<List<Notification>>(_jsonOptions);
            if (data != null && data.Count != 0) 
            {
                var result = await _service.Create(data);
                _logger.LogInformation("{result} rows were updated", result.Count);
                if (result.Count == 0) return Created();

                var url = "api/system/notification?" + string.Join("&", result.Select(x => $"id={x.Id}"));
                return Created(url, result);
            }
            else
            {
                return BadRequest("data not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{message}", ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete]
    [Authorize("Delete")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete([FromHeader] string authorization, [FromHeader] string userAgent, [FromBody] JsonElement json)
    {
        _service.UserId = _jwtHelper.GetUserId(authorization);
        _service.RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        _service.UserAgent = userAgent;

        var data = json.Deserialize<List<long>>(_jsonOptions);
        if (data != null && data.Count != 0) 
        {
            var result = await _service.Delete(data);
            _logger.LogInformation("{result} rows were deleted", result);
            return NoContent();
        }
        else
        {
            return BadRequest("data not found");
        }
    }
}
