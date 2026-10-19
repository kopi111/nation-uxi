using System.Text.Json.Serialization;
using UX.Shared.Common;

namespace UX.Shared.Config;

public class AgentEffectiveConfig
{
    [JsonPropertyName("pullConfigEverySeconds")]
    public int PullConfigEverySeconds { get; set; } = 60;

    [JsonPropertyName("enableInventory")]
    public bool EnableInventory { get; set; } = true;

    [JsonPropertyName("inventoryIntervalMinutes")]
    public int InventoryIntervalMinutes { get; set; } = 60;

    [JsonPropertyName("ingest")]
    public IngestConfig Ingest { get; set; } = new();

    [JsonPropertyName("wifiPolicy")]
    public WifiPolicy WifiPolicy { get; set; } = new();

    [JsonPropertyName("targets")]
    public List<ProbeTarget> Targets { get; set; } = new();

    public ValidationErrors Validate()
    {
        var errors = new ValidationErrors();

        if (PullConfigEverySeconds < 10)
            errors.AddError("PullConfigEverySeconds must be at least 10");

        if (InventoryIntervalMinutes < 5)
            errors.AddError("InventoryIntervalMinutes must be at least 5");

        if (string.IsNullOrWhiteSpace(Ingest.Url))
            errors.AddError("Ingest URL is required");

        return errors;
    }
}

public class IngestConfig
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = "https://ux-ingest/api/ingest/metrics";

    [JsonPropertyName("tokenTtlHint")]
    public string TokenTtlHint { get; set; } = "900s";
}

public class WifiPolicy
{
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "single";

    [JsonPropertyName("primaryAdapterId")]
    public string PrimaryAdapterId { get; set; } = "auto";

    [JsonPropertyName("secondaryAdapterId")]
    public string SecondaryAdapterId { get; set; } = "auto";

    [JsonPropertyName("secondaryRole")]
    public string SecondaryRole { get; set; } = "scanTelemetry";

    [JsonPropertyName("scanCadenceSeconds")]
    public int ScanCadenceSeconds { get; set; } = 120;
}

public class ProbeTarget
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; } = string.Empty;

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;
}
