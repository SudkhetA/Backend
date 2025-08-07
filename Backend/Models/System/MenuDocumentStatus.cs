using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.System;

[Table(nameof(MenuDocumentStatus), Schema = "system")]
[PrimaryKey(nameof(MenuId), nameof(DocumentStatusId))]
[Comment("สถานะเอกสารที่เกี่ยวของกับเมนู")]
public class MenuDocumentStatus
{
    [Required]
    public long MenuId { get; set; }

    [Required]
    public long DocumentStatusId { get; set; }

    public DateTime? CreatedDate { get; set; } = DateTime.Now;

    public long? CreatedBy { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(CreatedBy))]
    public virtual User? UserCreatedBy { get; set; }

    [Required]
    [ForeignKey(nameof(MenuId))]
    public virtual Menu Menu { get; private set; } = null!;

    [Required]
    [ForeignKey(nameof(DocumentStatusId))]
    public virtual DocumentStatus DocumentStatus { get; private set; } = null!;
}
