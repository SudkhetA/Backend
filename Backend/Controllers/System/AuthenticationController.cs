using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.System;

[Route("api/system/[controller]/[action]")]
[ApiController]
public class AuthenticationController(ILogger<AuthenticationController> _logger, Services.System.AuthenticationService authenticationService) : ControllerBase
{
    
    [HttpPost]
    [AllowAnonymous]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] JsonElement body)
    {
        var username = body.GetString("username");
        var password = body.GetString("password");

        _logger.LogInformation("Login by \"{username}\"", username);

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            _logger.LogWarning("username or password is missing");
            return Unauthorized();
        }

        var result = await authenticationService.LoginAsync(username, password);
        if (result == null)
        {
            _logger.LogWarning("username or password incorrect");
            return Unauthorized();
        }

        HttpContext.Response.Headers.Append("set-cookie", $"access_token={result["access_token"]}; Path=/; Expires={result["access_token_expires"]}; HttpOnly; Secure; SameSite=Strict;");
        HttpContext.Response.Headers.Append("set-cookie", $"refresh_token={result["refresh_token"]}; Path=/; Expires={result["refresh_token_expires"]}; HttpOnly; Secure; SameSite=Strict;");

        _logger.LogInformation("login successful");
        return Ok(result);
    }

    [HttpGet]
    [Authorize("Refresh", AuthenticationSchemes = "Refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshToken([FromHeader] string authorization) 
    {
        var result = await authenticationService.RefreshTokenAsync(authorization);

        HttpContext.Response.Headers.Append("set-cookie", $"access_token={result["access_token"]}; Path=/; Expires={result["access_token_expires"]}; HttpOnly; Secure; SameSite=Strict;");
        HttpContext.Response.Headers.Append("set-cookie", $"refresh_token={result["refresh_token"]}; Path=/; Expires={result["refresh_token_expires"]}; HttpOnly; Secure; SameSite=Strict;");

        _logger.LogInformation("refresh token successful");
        return Ok(result);
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult PagePermission([FromHeader] string authorization)
    {
        return Ok(authenticationService.PagePermission(authorization));
    }

}
