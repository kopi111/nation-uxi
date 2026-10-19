using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UX.ControlPlane.Data.Entities;

[Table("ConfigVersions")]
public class ConfigVersion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [MaxLength(16)]
    public string ScopeType { get; set; } = string.Empty;

    public Guid ScopeId { get; set; }

    [Required]
    public string ConfigJson { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string Hash { get; set; } = string.Empty;

    public DateTime PublishedUtc { get; set; }

    [Required]
    [MaxLength(200)]
    public string PublishedBy { get; set; } = string.Empty;
}
