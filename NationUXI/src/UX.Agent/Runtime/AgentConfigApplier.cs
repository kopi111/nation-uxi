using UX.Shared.Config;
using UX.Shared.Contracts;

namespace UX.Agent.Runtime;

public class AgentConfigApplier
{
    private readonly ILogger<AgentConfigApplier> _logger;
    private readonly StatusCache _statusCache;

    private long _currentConfigVersion;
    private AgentEffectiveConfig? _currentConfig;

    public AgentConfigApplier(ILogger<AgentConfigApplier> logger, StatusCache statusCache)
    {
        _logger = logger;
        _statusCache = statusCache;
    }

    public async Task ApplyConfigAsync(AgentEffectiveConfig config, long version, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentConfigVersion == version && _currentConfig != null)
            {
                _logger.LogDebug("Config version {Version} already applied", version);
                return;
            }

            _logger.LogInformation("Applying new config version {Version}", version);

            if (_currentConfig != null)
                LogConfigChanges(_currentConfig, config);

            _currentConfig = config;
            _currentConfigVersion = version;

            var existing = await _statusCache.GetCurrentStatusAsync(cancellationToken);
            await _statusCache.UpdateConfigAsync(new AgentCheckinResponse
            {
                EffectiveConfigVersion = version,
                EffectiveConfig = config,
                CanSend = existing.CanSend
            }, cancellationToken);

            _logger.LogInformation("Config applied: PullInterval={Pull}s, InventoryInterval={Inv}m",
                config.PullConfigEverySeconds, config.InventoryIntervalMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply config");
        }
    }

    private void LogConfigChanges(AgentEffectiveConfig oldConfig, AgentEffectiveConfig newConfig)
    {
        if (oldConfig.PullConfigEverySeconds != newConfig.PullConfigEverySeconds)
            _logger.LogInformation("Config changed: PullInterval {Old}s -> {New}s", oldConfig.PullConfigEverySeconds, newConfig.PullConfigEverySeconds);

        if (oldConfig.EnableInventory != newConfig.EnableInventory)
            _logger.LogInformation("Config changed: EnableInventory {Old} -> {New}", oldConfig.EnableInventory, newConfig.EnableInventory);

        if (oldConfig.Ingest.Url != newConfig.Ingest.Url)
            _logger.LogInformation("Config changed: Ingest URL changed");
    }
}
