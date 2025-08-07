using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.System;

[Table(nameof(UserNotification), Schema = "system")]
[PrimaryKey(nameof(UserId), nameof(NotificationId))]
[Comment("การแจ้งที่เกี่ยวข้องกับผู้ใช้งาน")]
public class UserNotification
{
    [Required]
    public long UserId { get; set; }

    [Required]
    public long NotificationId { get; set; }

    public DateTime? CreatedDate { get; set; } = DateTime.Now;

    public long? CreatedBy { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    public bool IsRead { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public virtual User? UserCreatedBy { get; set; }

    [Required]
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; private set; } = null!;

    [Required]
    [ForeignKey(nameof(NotificationId))]
    public virtual Notification Notification { get; private set; } = null!;
}
