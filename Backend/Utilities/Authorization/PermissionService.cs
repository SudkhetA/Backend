using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Backend.Models;
using Backend.Models.System;

namespace Backend.Utilities.Authorization
{
    public class PermissionService(IMemoryCache _memoryCache, DataContext _context)
    {
        public List<RoleMenu> GetApiPermission(long roleId)
        {
            if (_memoryCache.TryGetValue($"ApiPermission_{roleId}", out List<RoleMenu>? permissions)) return permissions!;
            permissions = [.. _context.RoleMenus
                .Where(x => x.RoleId == roleId
                    && x.IsActive == true)
                .Include(x => x.Menu)
                .ThenInclude(x => x!.MenuType)
                .Where(x => x.Menu!.MenuType!.Name == EnumMenuType.Api)];

            var options = new MemoryCacheEntryOptions();
            options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
            _memoryCache.Set($"ApiPermission_{roleId}", permissions, options);

            return permissions!;
        }

        public void ClearApiPermissionCache(long roleId)
        {
            _memoryCache.Remove($"ApiPermission_{roleId}");
        }

        public List<RoleMenu> GetPagePermission(long roleId)
        {
            if (_memoryCache.TryGetValue($"PagePermission_{roleId}", out List<RoleMenu>? permissions)) return permissions!;
            permissions = [.. _context.RoleMenus
                .Where(x => x.RoleId == roleId
                    && x.IsActive == true)
                .Include(x => x.Menu)
                .ThenInclude(x => x!.MenuType)
                .Where(x => x.Menu!.MenuType!.Name == EnumMenuType.Page)];

            var options = new MemoryCacheEntryOptions();
            options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
            _memoryCache.Set($"PagePermission_{roleId}", permissions, options);

            return permissions!;
        }

        public void ClearPagePermissionCache(long roleId)
        {
            _memoryCache.Remove($"PagePermission_{roleId}");
        }
    }
}
