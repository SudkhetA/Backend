using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
        var member = _context.Members
            .Where(x => x.Username!.ToLower() == username.ToLower()
                        && x.IsActive == true)
            .Include(x => x.Roles)
            .FirstOrDefault();

        if (member != null)
        {
            var salt = Convert.FromBase64String(member.SaltPassword);
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            var combine = new byte[passwordBytes.Length + salt.Length];
            passwordBytes.CopyTo(combine, 0);
            salt.CopyTo(combine, passwordBytes.Length);
            var hash = Convert.ToBase64String(SHA256.HashData(combine));

            if (member.Password.Equals(hash))
            {
                var roleId = member.Roles.Select(x => x.Id);

                var user = new UserInfo()
                {
                    MemberId = member.Id.ToString(),
                    Name = $"{member.FirstName} {member.LastName}",
                    Email = member.Email,
                    RoleIds = [.. roleId]
                };

                return user;
            }
        }
        return null;
    }
    
    private async Task<string> GenerateAccessTokenAsync(UserInfo user, DateTime expires)
    {
        var privateKey = _configuration["Authentication:Jwt-AccessToken:PrivateKey"] ?? throw new NullReferenceException("Jwt-AccessToken key not found");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var jwtHeader = new JwtHeader(credentials);
        var jwtPayload = new JwtPayload()
        {
            { "iss", _configuration["Authentication:Jwt-AccessToken:Issuer"] },
            { "aud", _configuration.GetSection("Authentication:Jwt-AccessToken:Audience").Get<string[]>() },
            { "exp", new DateTimeOffset(expires.ToUniversalTime()).ToUnixTimeSeconds() },
            { ClaimTypes.PrimarySid, Guid.NewGuid().ToString()},
            { ClaimTypes.NameIdentifier, user.MemberId},
            { ClaimTypes.Name, user.Name},
            { ClaimTypes.Role, user.RoleIds},
            { "email", user.Email }
        };

        var token = new JwtSecurityToken(jwtHeader, jwtPayload);

        await _sessionService.StoreSessionAsync($"access_{user.MemberId}", jwtPayload, expires - DateTime.Now);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    private async Task<string> GenerateRefreshTokenAsync(UserInfo user, DateTime expires)
    {
        var privateKey = _configuration["Authentication:Jwt-RefreshToken:PrivateKey"] ?? throw new NullReferenceException("Jwt-RefreshToken key not found");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var jwtHeader = new JwtHeader(credentials);
        var jwtPayload = new JwtPayload
        {
            { "iss", _configuration["Authentication:Jwt-RefreshToken:Issuer"] },
            { "aud", _configuration.GetSection("Authentication:Jwt-RefreshToken:Audience").Get<string[]>() },
            { "exp", new DateTimeOffset(expires.ToUniversalTime()).ToUnixTimeSeconds() },
            { ClaimTypes.PrimarySid, Guid.NewGuid().ToString()},
            { ClaimTypes.NameIdentifier, user.MemberId},
            { ClaimTypes.Name, user.Name},
            { ClaimTypes.Role, user.RoleIds},
            { "email", user.Email }
        };
        var token = new JwtSecurityToken(jwtHeader, jwtPayload);

        await _sessionService.StoreSessionAsync($"refresh_{user.MemberId}", jwtPayload, expires - DateTime.Now);
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

        var memberId = payload.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
        var name = payload.Claims.First(x => x.Type == ClaimTypes.Name).Value;
        var roleIds = JsonSerializer.Deserialize<List<long>>(payload.Claims.First(x => x.Type == ClaimTypes.Role).Value);

        var user = new UserInfo()
        {
            MemberId = memberId,
            Name = name,
            RoleIds = roleIds!
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
        public string MemberId { get; init; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<long> RoleIds { get; init; } = [];
    }
}