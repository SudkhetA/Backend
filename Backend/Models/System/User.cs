using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models.System;

[Table(nameof(User), Schema = "system")]
[Index(nameof(Username), IsUnique = true)]
[Comment("ผู้ใช้งาน")]
public class User : ModelBase
{
    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string Username { get; set; } = null!;

    [JsonIgnore]
    [Required]
    [StringLength(44)]
    [Unicode(false)]
    public string Password { get; set; } = null!;

    [JsonIgnore]
    [Required]
    [StringLength(24)]
    [Unicode(false)]
    public string SaltPassword { get; set; } = null!;
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = null!;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Email { get; set;} = null!;

    #region User
    [JsonIgnore]
    [InverseProperty(nameof(MemberCreatedBy))]
    public virtual ICollection<User>? MembersCreatedBy { get; } = [];

    [JsonIgnore]
    [InverseProperty(nameof(MemberUpdatedBy))]
    public virtual ICollection<User>? MembersUpdatedBy { get; } = [];
    #endregion

    #region Role
    [JsonIgnore]
    [InverseProperty(nameof(MemberCreatedBy))]
    public virtual ICollection<Role>? RoleCreatedBy { get; } = [];

    [JsonIgnore]
    [InverseProperty(nameof(MemberUpdatedBy))]
    public virtual ICollection<Role>? RoleUpdatedBy { get; } = [];
    #endregion

    #region Menu
    [JsonIgnore]
    [InverseProperty(nameof(MemberCreatedBy))]
    public virtual ICollection<Menu> MenuCreatedBy { get; } = [];

    [JsonIgnore]
    [InverseProperty(nameof(MemberUpdatedBy))]
    public virtual ICollection<Menu> MenuUpdatedBy { get; } = [];
    #endregion

    #region MenuType
    [JsonIgnore]
    [InverseProperty(nameof(MemberCreatedBy))]
    public virtual ICollection<MenuType> MenuTypeCreatedBy { get; } = [];

    [JsonIgnore]
    [InverseProperty(nameof(MemberUpdatedBy))]
    public virtual ICollection<MenuType> MenuTypeUpdatedBy { get; } = [];
    #endregion

    #region DocumentStatus
    [JsonIgnore]
    [InverseProperty(nameof(MemberCreatedBy))]
    public virtual ICollection<DocumentStatus> DocumentStatusCreatedBy { get; } = [];

    [JsonIgnore]
    [InverseProperty(nameof(MemberUpdatedBy))]
    public virtual ICollection<DocumentStatus> DocumentStatusUpdatedBy { get; } = [];
    #endregion

    #region Notification
    [JsonIgnore]
    [InverseProperty(nameof(MemberCreatedBy))]
    public virtual ICollection<Notification> NotificationCreatedBy { get; } = [];

    [JsonIgnore]
    [InverseProperty(nameof(MemberUpdatedBy))]
    public virtual ICollection<Notification> NotificationUpdatedBy { get; } = [];
    #endregion

    #region UserRole
    [JsonIgnore]
    public virtual ICollection<UserRole> MemberRoleCreatedBy { get; } = [];

    public virtual List<Role> Roles { get; } = [];
    public virtual List<UserRole> MemberRoles { get; } = [];
    #endregion

    #region UserDocumentStatus
    [JsonIgnore]
    public virtual ICollection<UserDocumentStatus> MemberDocumentCreatedBy { get; } = [];
    
    public virtual List<DocumentStatus> DocumentStatuses { get; } = [];
    public virtual List<UserDocumentStatus> MemberDocumentStatuses { get; } = [];
    #endregion

    #region UserNotification
    [JsonIgnore]
    public virtual ICollection<UserNotification> MemberNotificationCreatedBy { get; } = [];
    
    public virtual List<Notification> Notifications { get; } = [];
    public virtual List<UserNotification> MemberNotifications { get; } = [];
    #endregion
}

public class MemberSearch
{
    [FromQuery]
    public long[]? Id { get; set; }

    [FromQuery]
    public string[]? Username { get; set; }

    [FromQuery]
    public string[]? FirstName { get; set; }

    [FromQuery]
    public string[]? LastName { get; set; }

    [FromQuery]
    public string[]? Email { get; set; }

    [FromQuery]
    public long[]? RoleId { get; set; }

    [FromQuery]
    public long[]? DocumentStatusId { get; set; }

    [FromQuery]
    public long[]? NotificationId { get; set; }
}