using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Text.Json;
using Backend.Models.System;
using Backend.Services.System;
using Backend.Utilities.Helper;

namespace Backend.Controllers.System;

[Route("api/System/[controller]")]
[ApiController]
public class UserController(ILogger<UserController> _logger, JwtHelper _jwtHelper, UserService _service) : ControllerBase
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    [HttpGet]
    [Authorize("Read")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Read(MemberSearch search, [FromQuery] int page = 1, [FromQuery] int pageSize = 1000)
    {
        var data = await _service.Read(search, page, pageSize);
        
        return Ok(data);
    }

    [HttpPost]
    [Authorize("Create")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromHeader] string authorization, [FromHeader(Name = "User-Agent")] string userAgent, [FromBody] JsonElement json)
    {
        try
        {
            _service.MemberId = _jwtHelper.GetMemberId(authorization);
            _service.RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            _service.UserAgent = userAgent;

            var data = json.Deserialize<List<User>>(_jsonOptions);
            if (data != null && data.Count != 0) 
            {
                var result = await _service.Create(data);
                _logger.LogInformation("{result} rows were created", result.Count);

                var url = "api/system/member?" + string.Join("&", result.Select(x => $"id={x.Id}"));
                return Created(url, null);
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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromHeader] string authorization, [FromHeader(Name = "User-Agent")] string userAgent, [FromBody] JsonElement json)
    {
        try
        {
            _service.MemberId = _jwtHelper.GetMemberId(authorization);
            _service.RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            _service.UserAgent = userAgent;

            var data = json.Deserialize<List<User>>(_jsonOptions);
            if (data != null && data.Count != 0) 
            {
                var result = await _service.Create(data);
                _logger.LogInformation("{result} rows were updated", result.Count);
                
                var url = "api/system/member?" + string.Join("&", result.Select(x => $"id={x.Id}"));
                return Created(url, null);
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
    public async Task<IActionResult> Delete([FromHeader] string authorization, [FromHeader(Name = "User-Agent")] string userAgent, [FromBody] JsonElement json)
    {
        try
        {
            _service.MemberId = _jwtHelper.GetMemberId(authorization);
            _service.RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "{message}", ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("role/{id}")]
    [Authorize("Read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRole([FromRoute] long id)
    {
        var result = await _service.GetRole(id);
        return Ok(result);
    }

    [HttpPost("role")]
    [Authorize("Create")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InsertMemberRole([FromHeader] string authorization, [FromHeader(Name = "User-Agent")] string userAgent, [FromBody] JsonElement json)
    {
        try
        {
            _service.MemberId = _jwtHelper.GetMemberId(authorization);
            _service.RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            _service.UserAgent = userAgent;

            var memberId = json.GetProperty("memberId").GetInt64();
            var roles = json.GetProperty("roles").Deserialize<List<Role>>(_jsonOptions);
            if (memberId != 0 && roles != null && roles.Count != 0) 
            {
                var result = await _service.InsertMemberRole(memberId, roles);
                _logger.LogInformation("{result} rows were updated", result.Count);
                return Created();
            }
            else
            {
                return BadRequest();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{message}", ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("role")]
    [Authorize("Update")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMemberRole([FromHeader] string authorization, [FromHeader(Name = "User-Agent")] string userAgent, [FromBody] JsonElement json)
    {
        try
        {
            _service.MemberId = _jwtHelper.GetMemberId(authorization);
            _service.RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            _service.UserAgent = userAgent;
            
            var memberId = json.GetProperty("memberId").GetInt64();
            var roles = json.GetProperty("roles").Deserialize<List<Role>>(_jsonOptions);
            if (memberId != 0 && roles != null && roles.Count != 0) 
            {
                var result = await _service.UpdateMemberRole(memberId, roles);
                _logger.LogInformation("{result} rows were updated", result.Count);
                return Created();
            }
            else
            {
                return BadRequest();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{message}", ex.Message);
            return BadRequest(ex.Message);
        }
    }
}
