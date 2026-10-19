namespace UX.Agent.Identity;

public class AgentIdentity
{
    public Guid AgentId { get; set; }
    public string MachineName { get; set; } = Environment.MachineName;
}
