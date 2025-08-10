using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Backend.Models;
using Backend.Models.System;
using Backend.Utilities.Authentication;
using Backend.Utilities.Authorization;
using Backend.Utilities.Helper;

namespace Backend.Services.System;

public class AuthenticationService(
    IConfiguration _configuration, 
    DataContext _context,
    JwtHelper _jwt,
    PermissionService _permission,
    SessionService _sessionService)
{
    private UserInfo? AuthenticateUser(string username, string password)
    {
        var user = _context.Users
            .Where(x => x.Username!.ToLower() == username.ToLower()
                        && x.IsActive == true)
            .Include(x => x.Roles)
            .FirstOrDefault();

        if (user != null)
        {
            var salt = Convert.FromBase64String(user.SaltPassword);
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            var combine = new byte[passwordBytes.Length + salt.Length];
            passwordBytes.CopyTo(combine, 0);
            salt.CopyTo(combine, passwordBytes.Length);
            var hash = Convert.ToBase64String(SHA256.HashData(combine));

            if (user.Password.Equals(hash))
            {
                var roleId = user.Roles.Select(x => x.Id);

                var userInfo = new UserInfo()
                {
                    UserId = user.Id.ToString(),
                    Name = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    RoleIds = [.. roleId]
                };

                return userInfo;
            }
        }
        return null;
    }
    
    private async Task<string> GenerateAccessTokenAsync(UserInfo user, DateTime expires)
    {
        var privateKey = _configuration["Authentication:Jwt-AccessToken:PrivateKey"] ?? throw new NullReferenceException("Jwt-AccessToken key not found");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

        var jwtHeader = new JwtHeader(credentials);
        var jwtPayload = new JwtPayload()
        {
            { "iss", _configuration["Authentication:Jwt-AccessToken:Issuer"] },
            { "aud", _configuration.GetSection("Authentication:Jwt-AccessToken:Audience").Get<string[]>() },
            { "exp", new DateTimeOffset(expires.ToUniversalTime()).ToUnixTimeSeconds() },
            { "sessionId", Guid.NewGuid().ToString()},
            { "userId", user.UserId},
            { "name", user.Name},
            { "role", user.RoleIds},
            { "email", user.Email }
        };

        var token = new JwtSecurityToken(jwtHeader, jwtPayload);

        await _sessionService.StoreSessionAsync($"access_{user.UserId}", jwtPayload, expires - DateTime.Now);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    private async Task<string> GenerateRefreshTokenAsync(UserInfo user, DateTime expires)
    {
        var privateKey = _configuration["Authentication:Jwt-RefreshToken:PrivateKey"] ?? throw new NullReferenceException("Jwt-RefreshToken key not found");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

        var jwtHeader = new JwtHeader(credentials);
        var jwtPayload = new JwtPayload
        {
            { "iss", _configuration["Authentication:Jwt-RefreshToken:Issuer"] },
            { "aud", _configuration.GetSection("Authentication:Jwt-RefreshToken:Audience").Get<string[]>() },
            { "exp", new DateTimeOffset(expires.ToUniversalTime()).ToUnixTimeSeconds() },
            { "sessionId", Guid.NewGuid().ToString()},
            { "userId", user.UserId},
            { "name", user.Name},
            { "role", user.RoleIds},
            { "email", user.Email }
        };
        var token = new JwtSecurityToken(jwtHeader, jwtPayload);

        await _sessionService.StoreSessionAsync($"refresh_{user.UserId}", jwtPayload, expires - DateTime.Now);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public async Task<Dictionary<string, string>?> LoginAsync(string username, string password)
    {
        var user = AuthenticateUser(username, password);
        if (user == null) return null;

        var accessTokenExpires = DateTime.Now.AddMinutes(30);
        var accessTokenTask  =  GenerateAccessTokenAsync(user, accessTokenExpires);

        var refreshTokenExpires = DateTime.Now.AddDays(1);
        var refreshTokenTask  = GenerateRefreshTokenAsync(user, refreshTokenExpires);

        await Task.WhenAll(accessTokenTask, refreshTokenTask);

        return new Dictionary<string, string>
        {
            { "access_token", await accessTokenTask },
            { "access_token_expires", accessTokenExpires.ToUniversalTime().ToString("dddd, dd MMM yyyy HH:mm:ss") + " GMT" },
            { "refresh_token", await refreshTokenTask },
            { "refresh_token_expires", refreshTokenExpires.ToUniversalTime().ToString("dddd, dd MMM yyyy HH:mm:ss") + " GMT" },
        };
    }

    public async Task<Dictionary<string, string>> RefreshTokenAsync(string authorization)
    {
        var token = authorization.Replace("Bearer ", "");
        var payload = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var userId = payload.Claims.First(x => x.Type == "userId").Value;
        var name = payload.Claims.First(x => x.Type == "name").Value;
        var roleIds = payload.Claims
            .Where(x => x.Type == "role")
            .Select(x => long.Parse(x.Value))
            .ToList();

        var user = new UserInfo()
        {
            UserId = userId,
            Name = name,
            RoleIds = roleIds
        };

        var accessTokenExpires = DateTime.Now.AddMinutes(30);
        var accessTokenTask = GenerateAccessTokenAsync(user, accessTokenExpires);

        var refreshTokenExpires = DateTime.Now.AddDays(1);
        var refreshTokenTask = GenerateRefreshTokenAsync(user, refreshTokenExpires);

        await Task.WhenAll(accessTokenTask, refreshTokenTask);

        return new Dictionary<string, string>
        {
            { "access_token", await accessTokenTask },
            { "access_token_expires", accessTokenExpires.ToUniversalTime().ToString("dddd, dd MMM yyyy HH:mm:ss") + " GMT" },
            { "refresh_token", await refreshTokenTask },
            { "refresh_token_expires", refreshTokenExpires.ToUniversalTime().ToString("dddd, dd MMM yyyy HH:mm:ss") + " GMT" },
        };
    }

    public List<RoleMenu> PagePermission(string authorization)
    {
        var roleIdList = _jwt.GetRoleId(authorization);

        var roleMenus = new List<RoleMenu>();
        foreach (var item in roleIdList)
        {
            roleMenus.AddRange(_permission.GetPagePermission(item));
        }

        return roleMenus;
    }

    private class UserInfo
    {
        public string UserId { get; init; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<long> RoleIds { get; init; } = [];
    }
}
