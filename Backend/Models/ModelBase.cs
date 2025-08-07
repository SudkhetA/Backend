using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Models.System;

namespace Backend.Models;

public abstract class ModelBase
{
    [Key]
    public long Id { get; set; }
    
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime? CreatedDate { get; set; } = DateTime.Now;
    
    public long? CreatedBy { get; set; }
    
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime? UpdatedDate { get; set; }
    
    public long? UpdatedBy { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(CreatedBy))]
    public virtual User? MemberCreatedBy { get; private set; }

    [ForeignKey(nameof(UpdatedBy))]
    public virtual User? MemberUpdatedBy { get; private set; }
}
