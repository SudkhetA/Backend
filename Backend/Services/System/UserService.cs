using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using Backend.Models;
using Backend.Models.System;

namespace Backend.Services.System;

public class UserService(DataContext _context) : ServiceBase<User, UserSearch>(_context)
{
    public override Task<ReadResult> Read(UserSearch search, int page, int pageSize)
    {
        var query = _context.Users.AsQueryable();

        if (search.RoleId?.Length > 0)
        {
            query = query.Where(x => _context.UserRoles.Any(a => a.UserId == x.Id && search.RoleId.Contains(a.RoleId)));
        }

        if (search.DocumentStatusId?.Length > 0)
        {
            query = query.Where(x => _context.UserDocumentStatuses.Any(a => a.UserId == x.Id && search.DocumentStatusId.Contains(a.DocumentStatusId)));
        }

        if (search.NotificationId?.Length > 0)
        {
            query = query.Where(x => _context.UserNotifications.Any(a => a.UserId == x.Id && search.NotificationId.Contains(a.NotificationId)));
        }

        query = query
            .Where(x => (search.Id == null || search.Id.Length == 0 || search.Id.Contains(x.Id))
                && (search.Username == null || search.Username.Length == 0 || search.Username.Contains(x.Username))
                && (search.FirstName == null || search.FirstName.Length == 0 || search.FirstName.Contains(x.FirstName))
                && (search.LastName == null || search.LastName.Length == 0 || search.LastName.Contains(x.LastName))
                && (search.Email == null || search.Email.Length == 0 || search.Email.Contains(x.Email)));

        var result = new ReadResult
        {
            Page = page,
            PageSize = pageSize,
            Count = query.Count(),
            Data = query.Skip((page - 1) * pageSize).Take(pageSize).ToList()
        };

        _ = TransactionLogRead(result.Data, "system." + nameof(User));

        return Task.FromResult(result);
    }
    public override async Task<List<User>> Create(List<User> entities)
    {
        if (entities.Count != 0)
        {
            var usernames = _context.Users.Where(x => entities.Select(a => a.Username).Contains(x.Username)).Select(x => x.Username);

            if (usernames.Any())
            {
                throw new Exception($"\"{string.Join(", ", usernames)}\" this username already exists");
            }

            foreach (var entity in entities)
            {
                if (!string.IsNullOrWhiteSpace(entity.Password))
                {
                    var salt = new byte[16];
                    RandomNumberGenerator.Fill(salt);

                    var passwordBytes = Encoding.UTF8.GetBytes(entity.Password);

                    var combine = new byte[passwordBytes.Length + salt.Length];
                    passwordBytes.CopyTo(combine, 0);
                    salt.CopyTo(combine, passwordBytes.Length);
                    var hash = SHA256.HashData(combine);

                    entity.Id = 0;
                    entity.Password = Convert.ToBase64String(hash);
                    entity.SaltPassword = Convert.ToBase64String(salt);
                    entity.CreatedBy = UserId;
                    entity.CreatedDate = DateTime.Now;
                }
            }

            await _context.Users.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            _ = TransactionLogCreate(entities, "system." + nameof(User));
        }

        entities.ForEach(x =>
        {
            x.Password = string.Empty; // Clear password before returning
            x.SaltPassword = string.Empty; // Clear salt password before returning
        });

        return entities;
    }

    public override async Task<List<User>> Update(List<User> entities)
    {
        if (entities.Count != 0)
        {
            var contain = entities.Select(x => new { x.Id, x.Username});

            var data = await _context.Users
                .Where(x => contain.Contains(new { x.Id, x.Username }))
                .ToListAsync();

            var usernames = data.Select(x => x.Username);

            if (usernames.Any())
            {
                throw new Exception($"\"{string.Join(", ", usernames)}\" this username already exists");
            }

            var storage = new List<User>();
            foreach (var entity in entities)
            {
                var item = data.Find(x => x.Id == entity.Id);

                if (item != null)
                {
                    if(!string.IsNullOrEmpty(entity.Password))
                    {
                        var salt = new byte[16];
                        RandomNumberGenerator.Fill(salt);

                        var passwordBytes = Encoding.UTF8.GetBytes(entity.Password);

                        var combine = new byte[passwordBytes.Length + salt.Length];
                        passwordBytes.CopyTo(combine, 0);
                        salt.CopyTo(combine, passwordBytes.Length);
                        var hash = SHA256.HashData(combine);

                        item.Password = Convert.ToBase64String(hash);
                        item.SaltPassword = Convert.ToBase64String(salt);
                    }

                    if(!string.IsNullOrEmpty(entity.FirstName))
                    {
                        item.FirstName = entity.FirstName;
                    }

                    if(!string.IsNullOrEmpty(entity.LastName))
                    {
                        item.LastName = entity.LastName;
                    }

                    if(!string.IsNullOrEmpty(entity.Email))
                    {
                        item.Email = entity.Email;
                    }

                    item.UpdatedBy = UserId;
                    item.UpdatedDate = DateTime.Now;

                    storage.Add(item);
                }
            }
            _context.Users.UpdateRange(storage);
            await _context.SaveChangesAsync();

            _ = TransactionLogUpdate(entities, storage, "system." + nameof(User));
            
            storage.ForEach(x =>
            {
                x.Password = string.Empty; // Clear password before returning
                x.SaltPassword = string.Empty; // Clear salt password before returning
            });
            return storage;
        }

        return [];
    }

    public async Task<List<Role>> GetRole(long id)
    {
        var result = from userRoleDb in _context.UserRoles
                     where userRoleDb.UserId == id
                     join roleDb in _context.Roles on userRoleDb.RoleId equals roleDb.Id
                     select roleDb;

        return await result.ToListAsync();
    }

    public async Task<List<UserRole>> InsertUserRole(long userId, List<Role> roles)
    {
        if (roles.Count != 0)
        {           
            var storage = new List<UserRole>();
            foreach(var role in roles)
            {
                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = role.Id!,
                };

                storage.Add(userRole);
            }

            await _context.UserRoles.AddRangeAsync(storage);
            await _context.SaveChangesAsync();
        }

        var result = await _context.UserRoles
            .Where(x => x.UserId == userId)
            .ToListAsync();

        return result;
    }

    public async Task<List<UserRole>> UpdateUserRole(long userId, List<Role> roles)
    {
        if (roles.Count != 0)
        {
            var deleteRow = _context.UserRoles.Where(x => x.UserId == userId);
            _context.UserRoles.RemoveRange(deleteRow);
            await _context.SaveChangesAsync();

            var storage = new List<UserRole>();
            foreach(var role in roles)
            {
                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = role.Id!,
                };

                storage.Add(userRole);
            }

            await _context.UserRoles.AddRangeAsync(storage);
            await _context.SaveChangesAsync();
        }

        var result = await _context.UserRoles
            .Where(x => x.UserId == userId)
            .ToListAsync();

        return result;
    }
}
