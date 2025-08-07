using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.System;

[Table(nameof(Notification), Schema = "system")]
[Comment("การแจ้งเตือน")]
public class Notification : ModelBase
{
    [Required]
    [StringLength(100)]
    public string Header { get; set; } = null!;

    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = null!;

    [StringLength(2048)]
    public string? LinkPage { get; set; }

    public virtual List<User> Users { get; } = [];
    public virtual List<UserNotification> UserNotifications { get; } = [];
}

public class NotificationSearch
{
    public long[]? Id { get; set; }
    public long[]? UserId { get; set; }
}
