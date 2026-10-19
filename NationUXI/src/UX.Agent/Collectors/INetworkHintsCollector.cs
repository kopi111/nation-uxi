using UX.Shared.Contracts;

namespace UX.Agent.Collectors;

public interface INetworkHintsCollector
{
    Task<NetworkHintsDto?> CollectAsync(CancellationToken cancellationToken = default);
}
