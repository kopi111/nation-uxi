using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UX.ControlPlane.Data.Entities;

[Table("AuditLog")]
public class AuditLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Actor { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Entity { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string EntityId { get; set; } = string.Empty;

    public DateTime Utc { get; set; }

    public string? BeforeJson { get; set; }

    public string? AfterJson { get; set; }
}
