using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Moq;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Backend.Models;
using Backend.Services.System;
using Backend.Utilities.Authentication;
using Backend.Utilities.Authorization;
using Backend.Utilities.Helper;

namespace Backend.Test.UnitTesting.Services.System;

public class AuthenticationServiceTest
{
    [Fact]
    public async Task CanLogin()
    {
        #region Arrange
        List<KeyValuePair<string, string>> configCollection =
        [
            new("Authentication:Jwt-AccessToken:PrivateKey", "1d5979050bf596acc0fc20f289f3511cd36fc8162a253b513c70516210f688a7"),
            new("Authentication:Jwt-AccessToken:Issuer", "localhost"),
            new("Authentication:Jwt-AccessToken:Audience:0", "localhost"),

            new("Authentication:Jwt-RefreshToken:PrivateKey", "cdafcfcf7afb68c2bf0cdd9d531cfbf30a742f6fa122038ae13ca2960e2be5b6"),
            new("Authentication:Jwt-RefreshToken:Issuer", "localhost"),
            new("Authentication:Jwt-RefreshToken:Audience:0", "localhost"),

            new("Encryption:PrivateKeyPassword", "ec701650d0c7f078b3c64744244dc2098551fc8d7623f29589fdec22075120bf")
        ];

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configCollection!)
            .Build();

        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: nameof(CanLogin))
            .Options;

        var salt = new byte[16];
        RandomNumberGenerator.Fill(salt);

        var passwordBytes = Encoding.UTF8.GetBytes("123456");

        var combine = new byte[passwordBytes.Length + salt.Length];
        passwordBytes.CopyTo(combine, 0);
        salt.CopyTo(combine, passwordBytes.Length);
        var hash = SHA256.HashData(combine);

        using var context = new DataContext(options);

        context.Users.Add(new() { Id = 1, FirstName = "admin", LastName = "admin", Username = "admin", Password = Convert.ToBase64String(hash), SaltPassword = Convert.ToBase64String(salt), Email = "admin@admin.com" });
        context.Roles.Add(new() { Id = 1, Name = "admin" });
        context.UserRoles.Add(new() { UserId = 1, RoleId = 1 });
        context.SaveChanges();

        var mockIMemoryCache = new Mock<IMemoryCache>();
        
        var mockRedisDb = new Mock<IDatabase>();
        var mockRedisConn = new Mock<IConnectionMultiplexer>();
        mockRedisConn.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(mockRedisDb.Object);
        var mockIHostEnvironment = new Mock<IHostEnvironment>();
        mockIHostEnvironment.Setup(x => x.EnvironmentName).Returns("Testing");
        var sessionService = new SessionService(mockRedisConn.Object, mockIHostEnvironment.Object);

        var service = new AuthenticationService(configuration,
            context,
            new JwtHelper(),
            new PermissionService(mockIMemoryCache.Object, context),
            sessionService
        );
        #endregion

        #region Act
        var result = await service.LoginAsync("admin", "123456");
        #endregion

        #region Assert
        result.Should().NotBeNull();
        result.Should().ContainKeys(["access_token", "access_token_expires", "refresh_token", "refresh_token_expires"]);
        #endregion
    }

    [Fact]
    public async Task CanRefreshToken()
    {
        #region Arrange
        List<KeyValuePair<string, string>> configCollection =
        [
            new("Authentication:Jwt-AccessToken:PrivateKey", "1d5979050bf596acc0fc20f289f3511cd36fc8162a253b513c70516210f688a7"),
            new("Authentication:Jwt-AccessToken:Issuer", "localhost"),
            new("Authentication:Jwt-AccessToken:Audience", "localhost"),

            new("Authentication:Jwt-RefreshToken:PrivateKey", "cdafcfcf7afb68c2bf0cdd9d531cfbf30a742f6fa122038ae13ca2960e2be5b6"),
            new("Authentication:Jwt-RefreshToken:Issuer", "localhost"),
            new("Authentication:Jwt-RefreshToken:Audience", "localhost"),

            new("Encryption:PrivateKeyPassword", "ec701650d0c7f078b3c64744244dc2098551fc8d7623f29589fdec22075120bf"),
        ];

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configCollection!)
            .Build();

        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: nameof(CanRefreshToken))
            .Options;

        using var context = new DataContext(options);

        var mockIMemoryCache = new Mock<IMemoryCache>();

        var mockRedisDb = new Mock<IDatabase>();
        var mockRedisConn = new Mock<IConnectionMultiplexer>();
        mockRedisConn.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(mockRedisDb.Object);
        var mockIHostEnvironment = new Mock<IHostEnvironment>();
        mockIHostEnvironment.Setup(x => x.EnvironmentName).Returns("Testing");
        var sessionService = new SessionService(mockRedisConn.Object, mockIHostEnvironment.Object);

        var service = new AuthenticationService(configuration,
            context,
            new JwtHelper(),
            new PermissionService(mockIMemoryCache.Object, context),
            sessionService
        );

        var privateKey = configuration["Authentication:Jwt-RefreshToken:PrivateKey"];
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "admin"),
            new(ClaimTypes.Role, "[1]")
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Authentication:Jwt-RefreshToken:Issuer"],
            audience: configuration["Authentication:Jwt-RefreshToken:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(1),
            signingCredentials: credentials);

        var authorization = $"Bearer {new JwtSecurityTokenHandler().WriteToken(token)}";
        #endregion

        #region Act
        var result = await service.RefreshTokenAsync(authorization);
        #endregion

        #region Assert
        result.Should().NotBeNull();
        result.Should().ContainKeys(["access_token", "access_token_expires", "refresh_token", "refresh_token_expires"]);
        result.Should().Contain(x => x.Key == "access_token_expires" && DateTime.Parse(x.Value) > DateTime.Now.AddMinutes(29));
        #endregion
    }
}
