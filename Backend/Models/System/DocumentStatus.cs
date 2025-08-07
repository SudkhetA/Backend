using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.System;

[Table(nameof(DocumentStatus), Schema = "system")]
[Comment("สถานะเอกสาร")]
public class DocumentStatus : ModelBase
{
    [Required]
    [StringLength(20)]
    public string Name { get; set; } = null!;

    public int? Sequence { get; set; }

    public virtual List<User> Members { get; } = [];
    public virtual List<UserDocumentStatus> MemberDocumentStatuses { get; } = [];

    public virtual List<Menu> Menus { get; } = [];
    public virtual List<MenuDocumentStatus> MenuDocumentStatuses { get; } = [];
}

public class DocumentStatusSearch
{
    public long[]? Id { get; set; }
    public string[]? Name { get; set; }
    public long[]? MemberId { get; set; }
    public long[]? MenuId { get; set; }
}