using Microsoft.EntityFrameworkCore;

namespace UX.ControlPlane.Services;

public class ConfigPublisherService
{
    private readonly UxDbContext _dbContext;
    private readonly ILogger<ConfigPublisherService> _logger;

    public ConfigPublisherService(UxDbContext dbContext, ILogger<ConfigPublisherService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ConfigVersion> GetOrCreateVersionAsync(string scopeType, Guid scopeId, string configJson, string publishedBy)
    {
        var hash = ConfigHash.ComputeSha256(configJson);

        var existing = await _dbContext.ConfigVersions
            .FirstOrDefaultAsync(v => v.ScopeType == scopeType && v.ScopeId == scopeId && v.Hash == hash);

        if (existing != null) return existing;

        var version = new ConfigVersion
        {
            ScopeType = scopeType,
            ScopeId = scopeId,
            ConfigJson = configJson,
            Hash = hash,
            PublishedUtc = DateTime.UtcNow,
            PublishedBy = publishedBy
        };

        _dbContext.ConfigVersions.Add(version);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Published new config version {VersionId} for {ScopeType}/{ScopeId}", version.Id, scopeType, scopeId);

        return version;
    }
}
