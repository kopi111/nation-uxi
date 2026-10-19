using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UX.ControlPlane.Data.Entities;

[Table("ConfigProfiles")]
public class ConfigProfile
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(400)]
    public string? Description { get; set; }

    [Required]
    public string ConfigJson { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; }

    [Required]
    [MaxLength(200)]
    public string CreatedBy { get; set; } = string.Empty;

    public virtual ICollection<ConfigAssignment> Assignments { get; set; } = new List<ConfigAssignment>();
}
