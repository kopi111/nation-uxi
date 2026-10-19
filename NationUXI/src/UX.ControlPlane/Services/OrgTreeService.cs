using Microsoft.EntityFrameworkCore;

namespace UX.ControlPlane.Services;

public class OrgTreeService
{
    private readonly UxDbContext _dbContext;
    private readonly AuditService _auditService;
    private readonly ILogger<OrgTreeService> _logger;

    public OrgTreeService(UxDbContext dbContext, AuditService auditService, ILogger<OrgTreeService> logger)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<OrgNode> CreateAsync(CreateOrgNodeRequest request, CancellationToken cancellationToken = default)
    {
        var node = new OrgNode
        {
            Name = request.Name,
            NodeType = request.NodeType,
            ParentId = request.ParentId
        };

        _dbContext.OrgNodes.Add(node);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            "OrgNodeCreated",
            "OrgNode",
            node.Id.ToString(),
            null,
            new { node.Name, node.NodeType, node.ParentId },
            cancellationToken);

        return node;
    }

    public async Task<List<OrgNodeDto>> GetTreeAsync(CancellationToken cancellationToken = default)
    {
        var allNodes = await _dbContext.OrgNodes.OrderBy(n => n.Name).ToListAsync(cancellationToken);
        var dict = allNodes.ToDictionary(n => n.Id);

        var roots = allNodes.Where(n => !n.ParentId.HasValue).ToList();
        return roots.Select(r => BuildDto(r, dict)).ToList();
    }

    public async Task<OrgNodeDto?> GetNodeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var allNodes = await _dbContext.OrgNodes.ToListAsync(cancellationToken);
        var dict = allNodes.ToDictionary(n => n.Id);

        if (!dict.TryGetValue(id, out var node)) return null;
        return BuildDto(node, dict);
    }

    private OrgNodeDto BuildDto(OrgNode node, Dictionary<Guid, OrgNode> allNodes)
    {
        var dto = new OrgNodeDto
        {
            Id = node.Id,
            Name = node.Name,
            NodeType = node.NodeType,
            ParentId = node.ParentId,
            FullPath = GetFullPath(node, allNodes),
            Children = new List<OrgNodeDto>()
        };

        var children = allNodes.Values.Where(n => n.ParentId == node.Id).OrderBy(n => n.Name).ToList();
        foreach (var child in children)
            dto.Children.Add(BuildDto(child, allNodes));

        return dto;
    }

    private string GetFullPath(OrgNode node, Dictionary<Guid, OrgNode> allNodes)
    {
        var parts = new List<string> { node.Name };
        var current = node;

        while (current.ParentId.HasValue && allNodes.TryGetValue(current.ParentId.Value, out var parent))
        {
            parts.Insert(0, parent.Name);
            current = parent;
        }

        return string.Join(" / ", parts);
    }
}
