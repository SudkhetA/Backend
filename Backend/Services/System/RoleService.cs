using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Models.System;

namespace Backend.Services.System;

public class RoleService(DataContext _context) : ServiceBase<Role, RoleSearch>(_context)
{
    public override Task<ReadResult> Read(RoleSearch search, int page, int pageSize)
    {
        var query = _context.Roles.AsQueryable();

        if (search.MemberId?.Length > 0)
        {
            query = query.Where(x => _context.MemberRoles.Any(a => a.RoleId == x.Id && search.MemberId.Contains(a.MemberId)));
        }

        if (search.MenuId?.Length > 0)
        {
            query = query.Where(x => _context.RoleMenus.Any(a => a.RoleId == x.Id && search.MenuId.Contains(a.RoleId)));
        }

        query = query
            .Where(x => (search.Id == null || search.Id.Length == 0 || search.Id.Contains(x.Id))
                && (search.Name == null || search.Name.Length == 0 || search.Name.Contains(x.Name)));

        var result = new ReadResult
        {
            Page = page,
            PageSize = pageSize,
            Count = query.Count(),
            Data = query.Skip((page - 1) * pageSize).Take(pageSize).ToList()
        };

        return Task.FromResult(result);
    }

    public override async Task<List<Role>> Create(List<Role> entities)
    {
        if (entities.Count != 0)
        {
            foreach (var entity in entities)
            {
                entity.Id = 0;
                entity.CreatedDate = DateTime.Now;
                entity.CreatedBy = MemberId;
            }

            await _context.Roles.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            _ = TransactionLogCreate(entities, "system." + nameof(Role));
        }

        return entities;
    }

    public override async Task<List<Role>> Update(List<Role> entities)
    {
        if (entities.Count != 0)
        {
            var contain = entities.Select(x => x.Id);
            var data = await _context.Roles
                .Where(x => contain.Contains(x.Id))
                .ToListAsync();

            var storage = new List<Role>();
            foreach(var entity in entities)
            {
                var item = data.Find(x => x.Id == entity.Id);

                if (item != null)
                {
                    if (!string.IsNullOrEmpty(item.Name))
                    {
                        item.Name = entity.Name;
                    }

                    item.UpdatedBy = MemberId;
                    item.UpdatedDate = DateTime.Now;

                    storage.Add(item);
                }
            }
            
            _context.Roles.UpdateRange(storage);
            await _context.SaveChangesAsync();

            _ = TransactionLogUpdate(entities, storage, "system." + nameof(Role));

            return storage;
        }

        return [];
    }

    public async Task<List<RoleMenu>> GetRoleMenu(long id)
    {
        var query = _context.RoleMenus
            .Where(x => x.RoleId == id)
            .Include(x => x.Menu);

        return await query.ToListAsync();
    }

    public async Task<List<RoleMenu>> InsertRoleMenu(long roleId, List<Menu> menus)
    {
        if (menus.Count != 0)
        {           
            var storage = new List<RoleMenu>();
            foreach(var menu in menus)
            {
                var roleMenu = new RoleMenu
                {
                    RoleId = roleId,
                    MenuId = menu.Id!,
                    CreatedBy = MemberId,
                    CreatedDate = DateTime.Now,
                };

                storage.Add(roleMenu);
            }

            await _context.RoleMenus.AddRangeAsync(storage);
            await _context.SaveChangesAsync();
        }

        var result = await _context.RoleMenus
            .Where(x => x.RoleId == roleId)
            .ToListAsync();

        return result;
    }

    public async Task<List<RoleMenu>> UpdateRoleMenu(long roleId, List<Menu> menus)
    {
        if (menus.Count != 0)
        {
            var deleteRow = _context.RoleMenus.Where(x => x.RoleId == roleId);
            _context.RoleMenus.RemoveRange(deleteRow);
            await _context.SaveChangesAsync();

            var storage = new List<RoleMenu>();
            foreach(var menu in menus)
            {
                var memberRole = new RoleMenu
                {
                    RoleId = roleId,
                    MenuId = menu.Id!,
                    CreatedBy = MemberId,
                    CreatedDate = DateTime.Now,
                };

                storage.Add(memberRole);
            }

            await _context.RoleMenus.AddRangeAsync(storage);
            await _context.SaveChangesAsync();
        }

        var result = await _context.RoleMenus
            .Where(x => x.RoleId == roleId)
            .ToListAsync();

        return result;
    }
}