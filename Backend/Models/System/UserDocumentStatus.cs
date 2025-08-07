using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.System;

[Table(nameof(UserDocumentStatus), Schema = "system")]
[PrimaryKey(nameof(MemberId), nameof(DocumentStatusId))]
[Comment("ผู้ใช้งานที่เกี่ยวข้องกับสถานะเอกสาร")]
public class UserDocumentStatus
{
    [Required]
    public long MemberId { get; set; }

    [Required]
    public long DocumentStatusId { get; set; }

    public DateTime? CreatedDate { get; set; } = DateTime.Now;

    public long? CreatedBy { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(CreatedBy))]
    public virtual User? MemberCreatedBy { get; set; }

    [Required]
    [ForeignKey(nameof(MemberId))]
    public virtual User User { get; private set; } = null!;

    [Required]
    [ForeignKey(nameof(DocumentStatusId))]
    public virtual DocumentStatus DocumentStatus { get; private set; } = null!;
}