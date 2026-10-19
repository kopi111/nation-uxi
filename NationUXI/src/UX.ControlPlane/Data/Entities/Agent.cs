using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UX.ControlPlane.Data.Entities;

[Table("Agents")]
public class Agent
{
    [Key]
    public Guid AgentId { get; set; }

    [Required]
    [MaxLength(128)]
    public string MachineName { get; set; } = string.Empty;

    public Guid? OrgNodeId { get; set; }

    [MaxLength(200)]
    public string? AssignedUser { get; set; }

    [Required]
    [MaxLength(32)]
    public string OsFamily { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string OsVersion { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string AgentVersion { get; set; } = string.Empty;

    public DateTime LastSeenUtc { get; set; }

    [MaxLength(64)]
    public string? LastIp { get; set; }

    [MaxLength(64)]
    public string? LastGateway { get; set; }

    [MaxLength(64)]
    public string? LastSsid { get; set; }

    [MaxLength(64)]
    public string? LastBssid { get; set; }

    public long LastAppliedConfigVersion { get; set; }

    public bool CanSend { get; set; }

    [Required]
    [MaxLength(32)]
    public string Status { get; set; } = "PendingAssignment";

    [ForeignKey(nameof(OrgNodeId))]
    public virtual OrgNode? OrgNode { get; set; }

    public virtual AgentTelemetryLatest? LatestTelemetry { get; set; }
}
