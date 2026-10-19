using Microsoft.EntityFrameworkCore;

namespace UX.ControlPlane.Services;

public class AgentService
{
    private readonly UxDbContext _dbContext;
    private readonly AuditService _auditService;
    private readonly ILogger<AgentService> _logger;

    public AgentService(UxDbContext dbContext, AuditService auditService, ILogger<AgentService> logger)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Agent> RegisterOrUpdateAsync(AgentRegisterRequest request, CancellationToken cancellationToken = default)
    {
        var agent = await _dbContext.Agents.FirstOrDefaultAsync(a => a.AgentId == request.AgentId, cancellationToken);
        var beforeState = agent != null ? new { agent.Status, agent.OrgNodeId } : null;

        if (agent == null)
        {
            agent = new Agent
            {
                AgentId = request.AgentId,
                Status = "PendingAssignment",
                CanSend = false
            };
            _dbContext.Agents.Add(agent);
        }

        agent.MachineName = request.MachineName;
        agent.OsFamily = request.OsFamily;
        agent.OsVersion = request.OsVersion;
        agent.AgentVersion = request.AgentVersion;
        agent.LastSeenUtc = DateTime.UtcNow;

        if (request.NetworkHints != null)
        {
            agent.LastIp = request.NetworkHints.IpAddresses?.FirstOrDefault();
            agent.LastGateway = request.NetworkHints.DefaultGateway;
            agent.LastSsid = request.NetworkHints.ConnectedSsid;
            agent.LastBssid = request.NetworkHints.ConnectedBssid;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            beforeState == null ? "AgentRegistered" : "AgentUpdated",
            "Agent",
            agent.AgentId.ToString(),
            beforeState,
            new { agent.Status, agent.OrgNodeId },
            cancellationToken);

        return agent;
    }

    public async Task<Agent?> UpdateCheckinAsync(AgentCheckinRequest request, CancellationToken cancellationToken = default)
    {
        var agent = await _dbContext.Agents.FirstOrDefaultAsync(a => a.AgentId == request.AgentId, cancellationToken);
        if (agent == null) return null;

        agent.MachineName = request.MachineName;
        agent.OsFamily = request.OsFamily;
        agent.OsVersion = request.OsVersion;
        agent.AgentVersion = request.AgentVersion;
        agent.LastSeenUtc = DateTime.UtcNow;

        if (request.NetworkHints != null)
        {
            agent.LastIp = request.NetworkHints.IpAddresses?.FirstOrDefault();
            agent.LastGateway = request.NetworkHints.DefaultGateway;
            agent.LastSsid = request.NetworkHints.ConnectedSsid;
            agent.LastBssid = request.NetworkHints.ConnectedBssid;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return agent;
    }

    public async Task<List<AgentDto>> GetAgentsListAsync(CancellationToken cancellationToken = default)
    {
        var agents = await _dbContext.Agents
            .OrderByDescending(a => a.LastSeenUtc)
            .ToListAsync(cancellationToken);

        var allOrgNodes = await _dbContext.OrgNodes.ToDictionaryAsync(n => n.Id, cancellationToken);

        return agents.Select(a => new AgentDto
        {
            AgentId = a.AgentId,
            MachineName = a.MachineName,
            OrgNodeId = a.OrgNodeId,
            OrgNodePath = GetOrgNodePath(a.OrgNodeId, allOrgNodes),
            AssignedUser = a.AssignedUser,
            OsFamily = a.OsFamily,
            OsVersion = a.OsVersion,
            AgentVersion = a.AgentVersion,
            LastSeenUtc = a.LastSeenUtc,
            LastIp = a.LastIp,
            CanSend = a.CanSend,
            Status = a.Status
        }).ToList();
    }

    public async Task<bool> AssignAgentAsync(Guid agentId, Guid orgNodeId, string? assignedUser, CancellationToken cancellationToken = default)
    {
        var agent = await _dbContext.Agents.FirstOrDefaultAsync(a => a.AgentId == agentId, cancellationToken);
        if (agent == null) return false;

        var before = new { agent.OrgNodeId, agent.AssignedUser, agent.Status };

        agent.OrgNodeId = orgNodeId;
        agent.AssignedUser = assignedUser;
        agent.Status = "Assigned";

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            "AgentAssigned",
            "Agent",
            agentId.ToString(),
            before,
            new { agent.OrgNodeId, agent.AssignedUser, agent.Status },
            cancellationToken);

        return true;
    }

    public async Task<bool> ToggleCanSendAsync(Guid agentId, bool canSend, CancellationToken cancellationToken = default)
    {
        var agent = await _dbContext.Agents.FirstOrDefaultAsync(a => a.AgentId == agentId, cancellationToken);
        if (agent == null) return false;

        var before = new { agent.CanSend };
        agent.CanSend = canSend;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            "AgentCanSendToggled",
            "Agent",
            agentId.ToString(),
            before,
            new { agent.CanSend },
            cancellationToken);

        return true;
    }

    private static string? GetOrgNodePath(Guid? orgNodeId, Dictionary<Guid, OrgNode> allNodes)
    {
        if (!orgNodeId.HasValue || !allNodes.TryGetValue(orgNodeId.Value, out var node))
            return null;

        var parts = new List<string>();
        var current = node;

        while (true)
        {
            parts.Insert(0, current.Name);
            if (!current.ParentId.HasValue) break;
            if (!allNodes.TryGetValue(current.ParentId.Value, out var parent)) break;
            current = parent;
        }

        return string.Join(" / ", parts);
    }
}
