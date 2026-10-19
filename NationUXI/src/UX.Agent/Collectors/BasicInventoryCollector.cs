using System.Reflection;
using UX.Shared.Contracts;

namespace UX.Agent.Collectors;

public class BasicInventoryCollector : IInventoryCollector
{
    private readonly INetworkHintsCollector _networkCollector;

    public BasicInventoryCollector(INetworkHintsCollector networkCollector)
    {
        _networkCollector = networkCollector;
    }

    public async Task<AgentRegisterRequest> CollectInventoryAsync(CancellationToken cancellationToken = default)
    {
        var osFamily = GetOsFamily();
        var osVersion = Environment.OSVersion.VersionString;
        var agentVersion = GetAgentVersion();
        var networkHints = await _networkCollector.CollectAsync(cancellationToken);

        return new AgentRegisterRequest
        {
            AgentId = Guid.Empty, // AgentWorker sets this from identity when registering
            MachineName = Environment.MachineName,
            OsFamily = osFamily,
            OsVersion = osVersion,
            AgentVersion = agentVersion,
            DomainWorkgroup = null,
            NetworkHints = networkHints
        };
    }

    public Task<NetworkHintsDto?> CollectNetworkHintsAsync(CancellationToken cancellationToken = default)
        => _networkCollector.CollectAsync(cancellationToken);

    private static string GetOsFamily()
    {
        if (OperatingSystem.IsWindows()) return "Windows";
        if (OperatingSystem.IsLinux()) return "Linux";
        if (OperatingSystem.IsMacOS()) return "macOS";
        return "Unknown";
    }

    private static string GetAgentVersion()
    {
        var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        return asm.GetName().Version?.ToString() ?? "0.0.0";
    }
}
