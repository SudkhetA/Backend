using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.System;

[Table(nameof(MenuType), Schema = "system")]
[Comment("ประเภทเมนู")]
public class MenuType : ModelBase
{
    [Required]
    [StringLength(10)]
    public EnumMenuType? Name { get; set; } = null!;

    [InverseProperty(nameof(Menu.MenuType))]
    public virtual ICollection<Menu> Menus { get; } = [];
}

public enum EnumMenuType
{
    Page,
    Api,
    Mobile
}
