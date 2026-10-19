using UX.Shared.Contracts;

namespace UX.Agent.Collectors;

public interface IInventoryCollector
{
    Task<AgentRegisterRequest> CollectInventoryAsync(CancellationToken cancellationToken = default);
    Task<NetworkHintsDto?> CollectNetworkHintsAsync(CancellationToken cancellationToken = default);
}
