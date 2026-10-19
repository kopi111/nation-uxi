namespace UX.Shared.Contracts;

public record LatestTelemetryDto
{
    public Guid AgentId { get; set; }
    public DateTime LastPayloadReceivedUtc { get; set; }
    public string LastPayloadJson { get; set; } = string.Empty;
}
