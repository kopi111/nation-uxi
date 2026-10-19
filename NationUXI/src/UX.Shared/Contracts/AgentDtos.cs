using UX.Shared.Config;

namespace UX.Shared.Contracts;

public record AgentRegisterRequest
{
    public Guid AgentId { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public string OsFamily { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    public string AgentVersion { get; set; } = string.Empty;
    public string? DomainWorkgroup { get; set; }
    public NetworkHintsDto? NetworkHints { get; set; }
}

public record NetworkHintsDto
{
    public List<string>? IpAddresses { get; set; }
    public string? DefaultGateway { get; set; }
    public List<string>? DnsServers { get; set; }
    public string? ConnectedSsid { get; set; }
    public string? ConnectedBssid { get; set; }
}

public record AgentCheckinRequest
{
    public Guid AgentId { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public string OsFamily { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    public string AgentVersion { get; set; } = string.Empty;
    public string? DomainWorkgroup { get; set; }
    public NetworkHintsDto? NetworkHints { get; set; }
    public long CurrentConfigVersion { get; set; }
}

public record AgentCheckinResponse
{
    public bool CanSend { get; set; }
    public long EffectiveConfigVersion { get; set; }
    public string ConfigHash { get; set; } = string.Empty;
    public AgentEffectiveConfig EffectiveConfig { get; set; } = new();
    public int NextCheckInSeconds { get; set; } = 60;
}

public record SubmitTelemetryRequest
{
    public Guid AgentId { get; set; }
    public DateTime CollectedUtc { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
    public Dictionary<string, object> Inventory { get; set; } = new();
    public NetworkHintsDto? NetworkHints { get; set; }
}

public record SubmitTelemetryResponse
{
    public bool Accepted { get; set; }
    public string? Message { get; set; }
}
