using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models.System;

[Table(nameof(UserRole), Schema = "system")]
[PrimaryKey(nameof(UserId), nameof(RoleId))]
[Comment("บทบาทที่เกี่ยวข้องกับผู้ใช้งาน")]
public class UserRole
{
    [Required]
    public long UserId { get; set; }

    [Required]
    public long RoleId { get; set; }

    public DateTime? CreatedDate { get; set; } = DateTime.Now;

    public long? CreatedBy { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(CreatedBy))]
    public virtual User? UserCreatedBy { get; set; }
    
    [Required]
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; private set; } = null!;

    [Required]
    [ForeignKey(nameof(RoleId))]
    public virtual Role Role { get; private set; } = null!;
}

