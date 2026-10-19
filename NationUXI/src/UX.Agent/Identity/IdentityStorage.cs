using UX.Agent.Storage;

namespace UX.Agent.Identity;

public class IdentityStorage
{
    private readonly FileStore _fileStore;
    private readonly ILogger<IdentityStorage> _logger;

    public IdentityStorage(FileStore fileStore, ILogger<IdentityStorage> logger)
    {
        _fileStore = fileStore;
        _logger = logger;
    }

    public async Task<AgentIdentity> LoadOrCreateAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _fileStore.ReadAsync<AgentIdentity>(Paths.GetIdentityFilePath(), cancellationToken);
        if (existing != null && existing.AgentId != Guid.Empty)
            return existing;

        var created = new AgentIdentity
        {
            AgentId = Guid.NewGuid(),
            MachineName = Environment.MachineName
        };

        await _fileStore.WriteAsync(Paths.GetIdentityFilePath(), created, cancellationToken);
        _logger.LogInformation("Created new identity {AgentId}", created.AgentId);
        return created;
    }
}
