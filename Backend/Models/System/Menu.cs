using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.System;

[Table(nameof(Menu), Schema = "system")]
[Comment("เมนู")]
public class Menu : ModelBase
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(2048)]
    public string? Path { get; set; }

    public int? Sequence { get; set; }

    [Required]
    public long MenuTypeId { get; set; }

    [ForeignKey(nameof(MenuTypeId))]
    public virtual MenuType? MenuType { get; private set; }

    public virtual List<Role> Roles { get; } = [];
    public virtual List<RoleMenu> RoleMenus { get; } = [];

    public virtual List<DocumentStatus> DocumentStatuses { get; } = [];
    public virtual List<MenuDocumentStatus> MenuDocumentStatuses { get; } = [];
}

public class MenuSearch
{
    public long[]? Id { get; set; }
    public string[]? Name { get; set; }
    public string[]? Path { get; set; }
    public long[]? MenuTypeId { get; set; }
    public long[]? RoleId { get; set; }
    public long[]? DocumentStatusId { get; set; }
}