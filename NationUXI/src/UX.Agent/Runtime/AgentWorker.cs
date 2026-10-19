using Microsoft.Extensions.Options;
using UX.Agent.Client;
using UX.Agent.Collectors;
using UX.Agent.Identity;
using UX.Shared.Contracts;
using UX.Shared.Config;

namespace UX.Agent.Runtime;

public class AgentWorker : BackgroundService
{
    private readonly ILogger<AgentWorker> _logger;
    private readonly IServiceProvider _services;
    private readonly AgentIdentity _identity;
    private readonly StatusCache _statusCache;
    private readonly IInventoryCollector _inventoryCollector;
    private readonly AgentConfigApplier _configApplier;
    private readonly IOptions<ControlPlaneOptions> _options;

    private ControlPlaneClient? _client;
    private long _currentConfigVersion;
    private AgentEffectiveConfig? _currentConfig;
    private DateTime _nextInventoryCollection = DateTime.UtcNow;
    private bool _registered;

    public AgentWorker(
        ILogger<AgentWorker> logger,
        IServiceProvider services,
        AgentIdentity identity,
        StatusCache statusCache,
        IInventoryCollector inventoryCollector,
        AgentConfigApplier configApplier,
        IOptions<ControlPlaneOptions> options)
    {
        _logger = logger;
        _services = services;
        _identity = identity;
        _statusCache = statusCache;
        _inventoryCollector = inventoryCollector;
        _configApplier = configApplier;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UX Agent starting. AgentId={AgentId} MachineName={MachineName}", _identity.AgentId, _identity.MachineName);
        await _statusCache.UpdateIdentityAsync(_identity, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCycleAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in monitoring cycle");
            }

            var delaySeconds = _currentConfig?.PullConfigEverySeconds ?? _options.Value.CheckinIntervalSeconds;
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
        }
    }

    private async Task RunCycleAsync(CancellationToken cancellationToken)
    {
        _client ??= _services.GetRequiredService<ControlPlaneClient>();

        var inventory = await _inventoryCollector.CollectInventoryAsync(cancellationToken);
        inventory.AgentId = _identity.AgentId; // fill

        AgentCheckinResponse? response;

        if (!_registered)
        {
            _logger.LogInformation("Registering with control plane at {Base}", _options.Value.BaseUrl);
            response = await _client.RegisterAsync(inventory, cancellationToken);

            if (response == null)
            {
                _logger.LogWarning("Registration failed, will retry");
                return;
            }

            _registered = true;
            _logger.LogInformation("Registration successful");
        }
        else
        {
            var checkin = new AgentCheckinRequest
            {
                AgentId = _identity.AgentId,
                MachineName = inventory.MachineName,
                OsFamily = inventory.OsFamily,
                OsVersion = inventory.OsVersion,
                AgentVersion = inventory.AgentVersion,
                DomainWorkgroup = inventory.DomainWorkgroup,
                NetworkHints = inventory.NetworkHints,
                CurrentConfigVersion = _currentConfigVersion
            };

            response = await _client.CheckinAsync(checkin, cancellationToken);
        }

        if (response == null)
        {
            if (_registered)
            {
                _registered = false;
                _logger.LogWarning("Check-in failed; will re-register next cycle");
            }
            return;
        }

        await _statusCache.UpdateLastCheckinAsync(DateTime.UtcNow, cancellationToken);

        if (response.EffectiveConfig != null && response.EffectiveConfigVersion != _currentConfigVersion)
        {
            await _configApplier.ApplyConfigAsync(response.EffectiveConfig, response.EffectiveConfigVersion, cancellationToken);
            _currentConfig = response.EffectiveConfig;
            _currentConfigVersion = response.EffectiveConfigVersion;
        }

        // Send telemetry periodically (using inventory interval)
        if (response.CanSend && ShouldCollectInventory())
            await SendTelemetryAsync(cancellationToken);

        // Persist CanSend (and config info) as seen on checkin
        await _statusCache.UpdateConfigAsync(response, cancellationToken);
    }

    private bool ShouldCollectInventory()
    {
        var now = DateTime.UtcNow;
        if (now >= _nextInventoryCollection)
        {
            var interval = TimeSpan.FromMinutes(_currentConfig?.InventoryIntervalMinutes ?? 60);
            _nextInventoryCollection = now + interval;
            return true;
        }
        return false;
    }

    private async Task SendTelemetryAsync(CancellationToken cancellationToken)
    {
        try
        {
            var telemetry = new SubmitTelemetryRequest
            {
                AgentId = _identity.AgentId,
                CollectedUtc = DateTime.UtcNow,
                Metrics = new Dictionary<string, object>
                {
                    ["cpu_usage_percent"] = Random.Shared.NextDouble() * 100,
                    ["memory_available_bytes"] = SafeGetAvailableMemory(),
                    ["uptime_seconds"] = Environment.TickCount64 / 1000,
                    ["process_count"] = SafeGetProcessCount()
                },
                Inventory = new Dictionary<string, object>
                {
                    ["processors"] = Environment.ProcessorCount,
                    ["os_version"] = Environment.OSVersion.VersionString,
                    ["machine_name"] = Environment.MachineName,
                    ["framework_version"] = Environment.Version.ToString()
                },
                NetworkHints = await _inventoryCollector.CollectNetworkHintsAsync(cancellationToken)
            };

            var ok = await _client!.SubmitTelemetryAsync(telemetry, cancellationToken);
            if (ok)
            {
                _logger.LogDebug("Telemetry submitted");
                await _statusCache.UpdateLastTelemetryAsync(telemetry, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send telemetry");
        }
    }

    private static long SafeGetAvailableMemory()
    {
        try { return GC.GetGCMemoryInfo().TotalAvailableMemoryBytes; }
        catch { return 0; }
    }

    private static int SafeGetProcessCount()
    {
        try { return System.Diagnostics.Process.GetProcesses().Length; }
        catch { return 0; }
    }
}
