using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Backend.Models.System;

namespace Backend.Utilities.Authorization
{
    public class CrudAuthorizationHandler(PermissionService _permissionService) : AuthorizationHandler<CrudAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CrudAuthorizationRequirement requirement)
        {
            if (context.Resource is DefaultHttpContext resourceHttpContext && context.User.Identity != null && context.User.Identity.IsAuthenticated) {
                var roleJson = context.User.FindFirst(x => x.Type == ClaimTypes.Role)!.Value;
                var roleIds = JsonSerializer.Deserialize<long[]>(roleJson)!;

                var roleMenu = new List<RoleMenu>();
                foreach(var roleId in roleIds)
                {
                    roleMenu.AddRange(_permissionService.GetApiPermission(roleId));
                }

                roleMenu = [.. roleMenu.Where(x => resourceHttpContext.Request.Path.StartsWithSegments(x.Menu!.Path, StringComparison.OrdinalIgnoreCase))];
                switch (requirement.Requirement)
                {
                    case "Create":

                        if (roleMenu.Any(x => x.IsCreate == true))
                        {
                            context.Succeed(requirement);
                        }
                        else
                        {
                            context.Fail(new AuthorizationFailureReason(this, "you can't create this resource."));
                        }
                        return Task.CompletedTask;
                    case "Read":
                        if (roleMenu.Any(x => x.IsRead == true))
                        {
                            context.Succeed(requirement);
                        }
                        else
                        {
                            context.Fail(new AuthorizationFailureReason(this, "you can't read this resource."));
                        }
                        return Task.CompletedTask;
                    case "Update":
                        if (roleMenu.Any(x => x.IsUpdate == true))
                        {
                            context.Succeed(requirement);
                        }
                        else
                        {
                            context.Fail(new AuthorizationFailureReason(this, "you can't update this resource."));
                        }
                        return Task.CompletedTask;
                    case "Delete":
                        if (roleMenu.Any(x => x.IsDelete == true))
                        {
                            context.Succeed(requirement);
                        }
                        else
                        {
                            context.Fail(new AuthorizationFailureReason(this, "you can't delete this resource."));
                        }
                        return Task.CompletedTask;
                }
            }
            return Task.CompletedTask;
        }
    }
}
