using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.System;

[Table(nameof(TransactionLog), Schema = "system")]
public class TransactionLog
{
    [Key]
    public long Id { get; set; }

    [Required]
    public DateTime TimeStamp { get; set; }

    [Required]
    public long MemberId { get; set; }

    [Required]
    public EnumOperationType OperationType { get; set; }

    [Required]
    [StringLength(100)]
    [Unicode(false)]
    public string TableName { get; set; } = null!;

    [Required]
    public long RecordId { get; set; }

    [MaxLength]
    public string? OldData { get; set; }
    
    [MaxLength]
    public string? NewData { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string? IpAddress { get; set; }

    [StringLength(1000)]
    [Unicode(false)]
    public string? UserAgent { get; set; }
}

public enum EnumOperationType
{
    Read,
    Create,
    Update,
    Delete
}