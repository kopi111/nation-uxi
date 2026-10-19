using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UX.Shared.Contracts;

namespace UX.ControlPlane.Data.Entities;

[Table("OrgNodes")]
public class OrgNode
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? ParentId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public OrgNodeType NodeType { get; set; }

    [ForeignKey(nameof(ParentId))]
    public virtual OrgNode? Parent { get; set; }

    public virtual ICollection<OrgNode> Children { get; set; } = new List<OrgNode>();
    public virtual ICollection<Agent> Agents { get; set; } = new List<Agent>();
}
