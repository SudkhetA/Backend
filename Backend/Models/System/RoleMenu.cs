using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models.System;

[Table(nameof(RoleMenu), Schema = "system")]
[PrimaryKey(nameof(RoleId), nameof(MenuId))]
[Comment("เมนูและสิทธิ์การใช้งานที่เกี่ยวข้องบทบาท")]
public class RoleMenu
{
    [Required]
    public long RoleId { get; set; }

    [Required]
    public long MenuId { get; set; }

    public DateTime? CreatedDate { get; set; } = DateTime.Now;

    public long? CreatedBy { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    public bool IsCreate { get; set; }
    public bool IsRead { get; set; }
    public bool IsUpdate { get; set; }
    public bool IsDelete { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public virtual User? MemberCreatedBy { get; set; }

    [Required]
    [ForeignKey(nameof(RoleId))]
    public virtual Role? Role { get; set; }

    [Required]
    [ForeignKey(nameof(MenuId))]
    public virtual Menu? Menu { get; set; }
}

