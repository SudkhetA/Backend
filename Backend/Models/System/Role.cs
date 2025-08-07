using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.System;

[Table(nameof(Role), Schema = "system")]
[Comment("่บทบาท")]
public class Role : ModelBase
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    public virtual List<User> Users { get; set; } = [];
    public virtual List<UserRole> UserRoles { get; set; } = [];

    public virtual List<Menu> Menus { get; set; } = [];
    public virtual List<RoleMenu> RoleMenus { get; set; } = [];
}

public class RoleSearch
{
    public long[]? Id { get; set; }
    public string[]? Name { get; set; }
    public long[]? UserId { get; set; }
    public long[]? MenuId { get; set; }
}
