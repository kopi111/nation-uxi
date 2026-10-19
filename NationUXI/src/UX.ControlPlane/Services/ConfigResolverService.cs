using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UX.Shared.Common;

namespace UX.ControlPlane.Services;

public class ConfigResolverService
{
    private readonly UxDbContext _dbContext;
    private readonly ConfigPublisherService _publisher;
    private readonly ILogger<ConfigResolverService> _logger;

    public ConfigResolverService(UxDbContext dbContext, ConfigPublisherService publisher, ILogger<ConfigResolverService> logger)
    {
        _dbContext = dbContext;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<(AgentEffectiveConfig Config, ConfigVersion? Version)> ResolveEffectiveConfigAsync(Agent agent)
    {
        try
        {
            var assignments = await GetApplicableAssignmentsAsync(agent);
            if (!assignments.Any())
                return (GetDefaultConfig(), null);

            // Precedence: Agent > OrgNode, then Priority, then newest EffectiveFromUtc
            var selected = assignments
                .OrderBy(a => a.ScopeType == "Agent" ? 0 : 1)
                .ThenBy(a => a.Priority)
                .ThenByDescending(a => a.EffectiveFromUtc ?? DateTime.MinValue)
                .First();

            var config = ParseConfig(selected.ConfigProfile.ConfigJson);
            if (config == null)
                return (GetDefaultConfig(), null);

            var version = await _publisher.GetOrCreateVersionAsync(
                selected.ScopeType,
                selected.ScopeId,
                selected.ConfigProfile.ConfigJson,
                "System");

            return (config, version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving config for agent {AgentId}", agent.AgentId);
            return (GetDefaultConfig(), null);
        }
    }

    private async Task<List<ConfigAssignment>> GetApplicableAssignmentsAsync(Agent agent)
    {
        var now = DateTime.UtcNow;
        var assignments = new List<ConfigAssignment>();

        var agentAssignments = await _dbContext.ConfigAssignments
            .Include(a => a.ConfigProfile)
            .Where(a => a.ScopeType == "Agent" &&
                        a.ScopeId == agent.AgentId &&
                        a.Enabled &&
                        (!a.EffectiveFromUtc.HasValue || a.EffectiveFromUtc.Value <= now))
            .ToListAsync();

        assignments.AddRange(agentAssignments);

        if (agent.OrgNodeId.HasValue)
        {
            var orgAssignments = await _dbContext.ConfigAssignments
                .Include(a => a.ConfigProfile)
                .Where(a => a.ScopeType == "OrgNode" &&
                            a.ScopeId == agent.OrgNodeId.Value &&
                            a.Enabled &&
                            (!a.EffectiveFromUtc.HasValue || a.EffectiveFromUtc.Value <= now))
                .ToListAsync();

            assignments.AddRange(orgAssignments);
        }

        return assignments;
    }

    private AgentEffectiveConfig? ParseConfig(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<AgentEffectiveConfig>(json, JsonOptions.Default);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse config JSON");
            return null;
        }
    }

    private static AgentEffectiveConfig GetDefaultConfig() => new()
    {
        PullConfigEverySeconds = 60,
        EnableInventory = true,
        InventoryIntervalMinutes = 60,
        Ingest = new IngestConfig { Url = "https://ux-ingest/api/ingest/metrics" },
        WifiPolicy = new WifiPolicy(),
        Targets = new List<ProbeTarget>()
    };
}
