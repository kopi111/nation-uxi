using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UX.ControlPlane.Data.Entities;

[Table("AgentTelemetryLatest")]
public class AgentTelemetryLatest
{
    [Key]
    public Guid AgentId { get; set; }

    public DateTime LastPayloadReceivedUtc { get; set; }

    public string LastPayloadJson { get; set; } = string.Empty;

    [ForeignKey(nameof(AgentId))]
    public virtual Agent Agent { get; set; } = null!;
}
