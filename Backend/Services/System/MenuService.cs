using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Models.System;

namespace Backend.Services.System;

public class MenuService(DataContext _context) : ServiceBase<Menu, MenuSearch>(_context)
{
    public override Task<ReadResult> Read(MenuSearch search, int page, int pageSize)
    {
        var query = _context.Menus.AsQueryable();

        if (search.RoleId?.Length > 0)
        {
            query = query.Where(x => _context.RoleMenus.Any(a => a.MenuId == x.Id && search.RoleId.Contains(a.RoleId)));
        }

        if (search.DocumentStatusId?.Length > 0)
        {
            query = query.Where(x => _context.MenuDocumentStatuses.Any(a => a.MenuId == x.Id && search.DocumentStatusId.Contains(a.DocumentStatusId)));
        }

        query = query
            .Where(x => (search.Id == null || search.Id.Length == 0 || search.Id.Contains(x.Id))
                && (search.Name == null || search.Name.Length == 0 || search.Name.Contains(x.Name))
                && (search.Path == null || search.Path.Length == 0 || search.Path.Contains(x.Path))
                && (search.MenuTypeId == null || search.MenuTypeId.Length == 0 || search.MenuTypeId.Contains(x.MenuTypeId)));

        var result = new ReadResult
        {
            Page = page,
            PageSize = pageSize,
            Count = query.Count(),
            Data = query.Skip((page - 1) * pageSize).Take(pageSize).ToList()
        };

        return Task.FromResult(result);
    }

    public override Task<List<Menu>> Create(List<Menu> entities)
    {
        throw new NotImplementedException();
    }

    public override Task<List<Menu>> Update(List<Menu> entities)
    {
        throw new NotImplementedException();
    }
}