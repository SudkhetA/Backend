using System.Security.Cryptography;
using System.Text;
using Backend.Models.System;
using Backend.Utilities.Helper;

namespace Backend.Models;

public class DataSeed
{
    public static void Seed(DataContext context, EncryptedHelper encrypted)
    {
        context.Database.EnsureCreated();

        var user = context.Users.FirstOrDefault(x => x.Username == "developer");
        if (user == null)
        {
            var salt = new byte[16];
            RandomNumberGenerator.Fill(salt);
            var passwordBytes = Encoding.UTF8.GetBytes("Admin@Developer");
            var combine = new byte[passwordBytes.Length + salt.Length];
            passwordBytes.CopyTo(combine, 0);
            salt.CopyTo(combine, passwordBytes.Length);
            var hash = SHA256.HashData(combine);

            user = new User
            {
                Username = "developer",
                Password = Convert.ToBase64String(hash),
                FirstName = "Sudkhet",
                LastName = "Authairat",
                Email = "sudkhet.a04@gmail.com",
                SaltPassword = Convert.ToBase64String(salt)
            };
            context.Users.Add(user);
            context.SaveChanges();

            user.CreatedBy = user.Id;
            context.Users.Update(user);
            context.SaveChanges();
        }

        var role = context.Roles.FirstOrDefault(x => x.Name == "Developer");
        if (role == null)
        {
            role = new Role
            {
                Name = "Developer",
                CreatedBy = user.Id,
            };
            context.Roles.Add(role);
            context.SaveChanges();
        }

        var userRole = context.UserRoles.FirstOrDefault(x => x.UserId == user.Id);
        if (userRole == null)
        {
            userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
            };
            context.UserRoles.Add(userRole);
            context.SaveChanges();
        }

        var apiMenuType = context.MenuTypes.FirstOrDefault(x => x.Name == EnumMenuType.Api);
        if (apiMenuType == null)
        {
            apiMenuType = new MenuType
            {
                Name = EnumMenuType.Api,
                CreatedBy = user.Id,
            };
            context.MenuTypes.Add(apiMenuType);
            context.SaveChanges();
        }

        var pageMenuType = context.MenuTypes.FirstOrDefault(x => x.Name == EnumMenuType.Page);
        if (pageMenuType == null)
        {
            pageMenuType = new MenuType
            {
                Name = EnumMenuType.Page,
                CreatedBy = user.Id,
            };
            context.MenuTypes.Add(pageMenuType);
            context.SaveChanges();
        }

        var apiMenuDefault = new List<(string name, string path)>
        {
            ("User", "/api/system/user"),
            ("Role", "/api/system/role"),
            ("Menu", "/api/system/menu")
        };

        var apiMenuQuery = context.Menus.Where(x => apiMenuDefault.Select(a => a.path).Contains(x.Path));
        if (apiMenuDefault.Count != apiMenuQuery.Count())
        {
            var apiMenuDb = apiMenuQuery.ToList();
            var data = new List<Menu>();
            foreach(var (name, path) in apiMenuDefault)
            {
                if (!apiMenuDb.Any(x => x.Path == path))
                {
                    var menu = new Menu
                    {
                        Name = name,
                        Path = path,
                        MenuTypeId = apiMenuType.Id,
                        CreatedBy = user.Id,
                    };
                    data.Add(menu);
                }
            }

            context.Menus.AddRange(data);
            context.SaveChanges();
        }

        var pageMenuDefault = new List<(string name, string path)>
        {
            ("Index", "/"),
            ("User", "/user"),
            ("UserInfo", "/user/info"),
            ("Role", "/role"),
            ("RoleInfo", "/role/info")
        };

        var pageMenuQuery = context.Menus.Where(x => pageMenuDefault.Select(a => a.path).Contains(x.Path));
        if (pageMenuDefault.Count != pageMenuQuery.Count())
        {
            var pageMenuDb = pageMenuQuery.ToList();
            var data = new List<Menu>();
            foreach(var (name, path) in pageMenuDefault)
            {
                if (!pageMenuDb.Any(x => x.Path == path))
                {
                    var menu = new Menu
                    {
                        Name = name,
                        Path = path,
                        MenuTypeId = pageMenuType.Id,
                        CreatedBy = user.Id,
                    };
                    data.Add(menu);
                }
            }

            context.Menus.AddRange(data);
            context.SaveChanges();
        }

        var roleMenus = context.RoleMenus.Where(x => x.RoleId == role.Id).ToList();
        if (roleMenus.Count == 0)
        {
            var apiMenuDb = apiMenuQuery.ToList();
            foreach(var item in apiMenuDb)
            {
                var roleMenu = new RoleMenu
                {
                    RoleId = role.Id,
                    MenuId = item.Id,
                    IsCreate = true,
                    IsRead = true,
                    IsUpdate = true,
                    IsDelete = true,
                    CreatedBy = user.Id,
                };

                roleMenus.Add(roleMenu);
            }

            var pageMenuDb = pageMenuQuery.ToList();
            foreach(var item in pageMenuDb)
            {
                var roleMenu = new RoleMenu
                {
                    RoleId = role.Id,
                    MenuId = item.Id,
                    IsCreate = true,
                    IsRead = true,
                    IsUpdate = true,
                    IsDelete = true,
                    CreatedBy = user.Id,
                };

                roleMenus.Add(roleMenu);
            }

            context.RoleMenus.AddRange(roleMenus);
            context.SaveChanges();
        }
    }
}
