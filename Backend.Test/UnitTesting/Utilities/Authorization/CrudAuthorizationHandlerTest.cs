using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System.Security.Claims;
using Backend.Models;
using Backend.Utilities.Authorization;

namespace Backend.Test.UnitTesting.Utilities.Authorization;

public class CrudAuthorizationHandlerTest
{
    [Fact]
    public void CanAuthAll()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "CrudAuthorizationHandlerTest1")
            .Options;

        using var context = new DataContext(options);
        context.Users.Add(new() { Id = 1, FirstName = "admin", LastName = "admin", Username = "admin", Password = "", Email = "", SaltPassword = "" });
        context.Roles.Add(new() { Id = 1, Name = "admin" });
        context.UserRoles.Add(new () { UserId = 1, RoleId = 1 });
        context.MenuTypes.Add(new () { Id = 1, Name = Models.System.EnumMenuType.Api});
        context.Menus.Add(new () { Id = 1, Name = "index", MenuTypeId = 1, Path = "/"});
        context.RoleMenus.Add(new () { RoleId = 1, MenuId = 1, IsRead = true, IsCreate = true, IsUpdate = true, IsDelete = true });
        context.SaveChanges();

        var mockIMemoryCache = new Mock<IMemoryCache>();
        var mockCacheEntry = new Mock<ICacheEntry>();

        mockIMemoryCache.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

        var permissionService = new PermissionService(mockIMemoryCache.Object, context);
        var action = new CrudAuthorizationHandler(permissionService);

        var create = new CrudAuthorizationRequirement("Create");
        var read = new CrudAuthorizationRequirement("Read");
        var update = new CrudAuthorizationRequirement("Update");
        var delete = new CrudAuthorizationRequirement("Delete");
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "admin admin"),
            new(ClaimTypes.Role, "[1]")
        };
        var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var defaultHttpContext = new DefaultHttpContext();
        defaultHttpContext.Request.Path = "/";

        var createAuth = new AuthorizationHandlerContext([create], claimsPrincipal, defaultHttpContext);
        var readAuth = new AuthorizationHandlerContext([read], claimsPrincipal, defaultHttpContext);
        var updateAuth = new AuthorizationHandlerContext([update], claimsPrincipal, defaultHttpContext);
        var deleteAuth = new AuthorizationHandlerContext([delete], claimsPrincipal, defaultHttpContext);

        // Act
        action.HandleAsync(createAuth);
        action.HandleAsync(readAuth);
        action.HandleAsync(updateAuth);
        action.HandleAsync(deleteAuth);

        // Assert
        createAuth.HasSucceeded.Should().BeTrue();
        readAuth.HasSucceeded.Should().BeTrue();
        updateAuth.HasSucceeded.Should().BeTrue();
        deleteAuth.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public void CanAuthReadWriteUpdate()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "CrudAuthorizationHandlerTest2")
            .Options;

        using var context = new DataContext(options);
        context.Users.Add(new() { Id = 1, FirstName = "admin", LastName = "admin", Username = "admin", Password = "", Email = "", SaltPassword = "" });
        context.Roles.Add(new() { Id = 1, Name = "admin" });
        context.UserRoles.Add(new () { UserId = 1, RoleId = 1 });
        context.MenuTypes.Add(new () { Id = 1, Name = Models.System.EnumMenuType.Api});
        context.Menus.Add(new () { Id = 1, Name = "index", MenuTypeId = 1, Path = "/"});
        context.RoleMenus.Add(new () { RoleId = 1, MenuId = 1, IsRead = true, IsCreate = true, IsUpdate = true, IsDelete = false });
        context.SaveChanges();

        var mockIMemoryCache = new Mock<IMemoryCache>();
        var mockCacheEntry = new Mock<ICacheEntry>();

        mockIMemoryCache.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

        var permissionService = new PermissionService(mockIMemoryCache.Object, context);
        var action = new CrudAuthorizationHandler(permissionService);

        var create = new CrudAuthorizationRequirement("Create");
        var read = new CrudAuthorizationRequirement("Read");
        var update = new CrudAuthorizationRequirement("Update");
        var delete = new CrudAuthorizationRequirement("Delete");
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "admin admin"),
            new(ClaimTypes.Role, "[1]")
        };
        var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var defaultHttpContext = new DefaultHttpContext();
        defaultHttpContext.Request.Path = "/";

        var createAuth = new AuthorizationHandlerContext([create], claimsPrincipal, defaultHttpContext);
        var readAuth = new AuthorizationHandlerContext([read], claimsPrincipal, defaultHttpContext);
        var updateAuth = new AuthorizationHandlerContext([update], claimsPrincipal, defaultHttpContext);
        var deleteAuth = new AuthorizationHandlerContext([delete], claimsPrincipal, defaultHttpContext);

        // Act
        action.HandleAsync(createAuth);
        action.HandleAsync(readAuth);
        action.HandleAsync(updateAuth);
        action.HandleAsync(deleteAuth);

        // Assert
        createAuth.HasSucceeded.Should().BeTrue();
        readAuth.HasSucceeded.Should().BeTrue();
        updateAuth.HasSucceeded.Should().BeTrue();
        deleteAuth.HasSucceeded.Should().BeFalse();
    }
}
