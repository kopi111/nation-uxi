using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UX.ControlPlane.Data.Entities;

[Table("ConfigAssignments")]
public class ConfigAssignment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(16)]
    public string ScopeType { get; set; } = string.Empty;

    public Guid ScopeId { get; set; }

    public Guid ConfigProfileId { get; set; }

    public int Priority { get; set; }

    public bool Enabled { get; set; } = true;

    public DateTime? EffectiveFromUtc { get; set; }

    [ForeignKey(nameof(ConfigProfileId))]
    public virtual ConfigProfile ConfigProfile { get; set; } = null!;
}
