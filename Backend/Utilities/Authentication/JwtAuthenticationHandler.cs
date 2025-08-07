using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Backend.Utilities.Helper;

namespace Backend.Utilities.Authentication;

public class JwtAuthenticationHandler
{
    public static void DefaultConfiguration(JwtBearerOptions options, ConfigurationManager configuration)
    {
        var jwtKey = configuration["Authentication:Jwt-AccessToken:PrivateKey"] ?? throw new NullReferenceException("JWT private key not found");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ValidIssuer = configuration["Authentication:Jwt-AccessToken:Issuer"],
            ValidAudiences = configuration.GetSection("Authentication:Jwt-AccessToken:Audience").Get<string[]>(),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var jwtHelper = new JwtHelper();

                var claimsPrincipal = context.Principal;
                var sessionId = claimsPrincipal?.FindFirst(claim => claim.Type == ClaimTypes.PrimarySid)?.Value;
                var userId = claimsPrincipal?.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(sessionId))
                {
                    context.Fail("User ID or Session ID could not be validated.");
                    return;
                }

                var sessionService = context.HttpContext.RequestServices.GetRequiredService<SessionService>();
                if (await sessionService.IsSessionExists($"access_{userId}", sessionId) == false)
                {
                    context.Fail("Session has expired.");
                    return;
                }
            },
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new { error = "Authentication failed." });
                return context.Response.WriteAsync(result);
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new { error = "Access denied." });
                return context.Response.WriteAsync(result);
            }
        };
    }

    public static void RefreshConfiguration(JwtBearerOptions options, ConfigurationManager configuration)
    {
        var jwtKey = configuration["Authentication:Jwt-RefreshToken:PrivateKey"] ?? throw new NullReferenceException("JWT private key not found");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ValidIssuer = configuration["Authentication:Jwt-RefreshToken:Issuer"],
            ValidAudiences = configuration.GetSection("Authentication:Jwt-RefreshToken:Audience").Get<string[]>(),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
        
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var jwtHelper = new JwtHelper();

                var claimsPrincipal = context.Principal;
                var sessionId = claimsPrincipal?.FindFirst(claim => claim.Type == ClaimTypes.PrimarySid)?.Value;
                var userId = claimsPrincipal?.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(sessionId))
                {
                    context.Fail("User ID or Session ID could not be validated.");
                    return;
                }

                var sessionService = context.HttpContext.RequestServices.GetRequiredService<SessionService>();
                if (await sessionService.IsSessionExists($"refresh_{userId}", sessionId) == false)
                {
                    context.Fail(new SecurityTokenException("Session has expired."));
                    return;
                }
            },
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                return Task.CompletedTask;
            }
        };

    }
}
