using UX.Shared.Config;
using UX.Shared.Contracts;

namespace UX.Agent.Runtime;

public class AgentStatus
{
    public Guid AgentId { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public DateTime? LastCheckinUtc { get; set; }
    public long CurrentConfigVersion { get; set; }
    public AgentEffectiveConfig? CurrentConfig { get; set; }
    public SubmitTelemetryRequest? LastTelemetry { get; set; }
    public DateTime LastTelemetryUtc { get; set; }
    public bool CanSend { get; set; }
}
