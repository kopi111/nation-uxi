using UX.Agent.Identity;
using UX.Agent.Storage;
using UX.Shared.Contracts;

namespace UX.Agent.Runtime;

public class StatusCache
{
    private readonly FileStore _fileStore;
    private readonly ILogger<StatusCache> _logger;
    private AgentStatus? _current;

    public StatusCache(FileStore fileStore, ILogger<StatusCache> logger)
    {
        _fileStore = fileStore;
        _logger = logger;
    }

    public async Task<AgentStatus> GetCurrentStatusAsync(CancellationToken cancellationToken = default)
    {
        if (_current != null) return _current;

        _current = await _fileStore.ReadAsync<AgentStatus>(Paths.GetStatusFilePath(), cancellationToken) ?? new AgentStatus();
        return _current;
    }

    public async Task UpdateIdentityAsync(AgentIdentity identity, CancellationToken cancellationToken = default)
    {
        var status = await GetCurrentStatusAsync(cancellationToken);
        status.AgentId = identity.AgentId;
        status.MachineName = identity.MachineName;
        await SaveAsync(cancellationToken);
    }

    public async Task UpdateConfigAsync(AgentCheckinResponse response, CancellationToken cancellationToken = default)
    {
        var status = await GetCurrentStatusAsync(cancellationToken);
        status.CurrentConfigVersion = response.EffectiveConfigVersion;
        status.CurrentConfig = response.EffectiveConfig;
        status.CanSend = response.CanSend;
        await SaveAsync(cancellationToken);
    }

    public async Task UpdateLastCheckinAsync(DateTime checkinTimeUtc, CancellationToken cancellationToken = default)
    {
        var status = await GetCurrentStatusAsync(cancellationToken);
        status.LastCheckinUtc = checkinTimeUtc;
        await SaveAsync(cancellationToken);
    }

    public async Task UpdateLastTelemetryAsync(SubmitTelemetryRequest telemetry, CancellationToken cancellationToken = default)
    {
        var status = await GetCurrentStatusAsync(cancellationToken);
        status.LastTelemetry = telemetry;
        status.LastTelemetryUtc = DateTime.UtcNow;
        await SaveAsync(cancellationToken);
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (_current != null)
            await _fileStore.WriteAsync(Paths.GetStatusFilePath(), _current, cancellationToken);
    }
}
