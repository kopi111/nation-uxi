namespace UX.Shared.Contracts;

public enum OrgNodeType
{
    National = 0,
    Area = 1,
    Division = 2,
    Station = 3,
    Unit = 4
}

public record CreateOrgNodeRequest
{
    public string Name { get; set; } = string.Empty;
    public OrgNodeType NodeType { get; set; }
    public Guid? ParentId { get; set; }
}

public record OrgNodeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public OrgNodeType NodeType { get; set; }
    public Guid? ParentId { get; set; }
    public string FullPath { get; set; } = string.Empty;
    public List<OrgNodeDto> Children { get; set; } = new();
}

public record AgentDto
{
    public Guid AgentId { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public Guid? OrgNodeId { get; set; }
    public string? OrgNodePath { get; set; }
    public string? AssignedUser { get; set; }
    public string OsFamily { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    public string AgentVersion { get; set; } = string.Empty;
    public DateTime LastSeenUtc { get; set; }
    public string? LastIp { get; set; }
    public bool CanSend { get; set; }
    public string Status { get; set; } = "PendingAssignment";
}

public record AssignAgentRequest
{
    public Guid AgentId { get; set; }
    public Guid OrgNodeId { get; set; }
    public string? AssignedUser { get; set; }
}

public record ToggleCanSendRequest
{
    public Guid AgentId { get; set; }
    public bool CanSend { get; set; }
}

public record CreateConfigProfileRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ConfigJson { get; set; } = string.Empty;
}

public record ConfigProfileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ConfigJson { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public record CreateConfigAssignmentRequest
{
    public string ScopeType { get; set; } = string.Empty;
    public Guid ScopeId { get; set; }
    public Guid ConfigProfileId { get; set; }
    public int Priority { get; set; }
    public DateTime? EffectiveFromUtc { get; set; }
}

public record ConfigAssignmentDto
{
    public Guid Id { get; set; }
    public string ScopeType { get; set; } = string.Empty;
    public Guid ScopeId { get; set; }
    public string ScopeName { get; set; } = string.Empty;
    public Guid ConfigProfileId { get; set; }
    public string ConfigProfileName { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool Enabled { get; set; }
    public DateTime? EffectiveFromUtc { get; set; }
}
