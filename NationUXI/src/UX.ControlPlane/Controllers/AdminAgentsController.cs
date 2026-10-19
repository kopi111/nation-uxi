using Microsoft.AspNetCore.Mvc;

namespace UX.ControlPlane.Controllers;

[ApiController]
[Route("api/admin/agents")]
public class AdminAgentsController : ControllerBase
{
    private readonly AgentService _agentService;
    private readonly TelemetryService _telemetryService;
    private readonly ILogger<AdminAgentsController> _logger;

    public AdminAgentsController(AgentService agentService, TelemetryService telemetryService, ILogger<AdminAgentsController> logger)
    {
        _agentService = agentService;
        _telemetryService = telemetryService;
        _logger = logger;
    }

    [HttpGet("list")]
    public async Task<ActionResult<List<AgentDto>>> ListAgents()
    {
        try
        {
            var agents = await _agentService.GetAgentsListAsync(HttpContext.RequestAborted);
            return Ok(agents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing agents");
            return StatusCode(500, new { error = "Failed to retrieve agents" });
        }
    }

    [HttpPost("{agentId:guid}/assign")]
    public async Task<IActionResult> AssignAgent(Guid agentId, [FromBody] AssignAgentRequest request)
    {
        try
        {
            if (agentId != request.AgentId)
                return BadRequest(new { error = "Agent ID mismatch" });

            var success = await _agentService.AssignAgentAsync(agentId, request.OrgNodeId, request.AssignedUser, HttpContext.RequestAborted);
            if (!success)
                return NotFound(new { error = "Agent not found" });

            return Ok(new { message = "Agent assigned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning agent {AgentId}", agentId);
            return StatusCode(500, new { error = "Failed to assign agent" });
        }
    }

    [HttpPost("{agentId:guid}/cansend")]
    public async Task<IActionResult> ToggleCanSend(Guid agentId, [FromBody] ToggleCanSendRequest request)
    {
        try
        {
            if (agentId != request.AgentId)
                return BadRequest(new { error = "Agent ID mismatch" });

            var success = await _agentService.ToggleCanSendAsync(agentId, request.CanSend, HttpContext.RequestAborted);
            if (!success)
                return NotFound(new { error = "Agent not found" });

            return Ok(new { message = $"CanSend set to {request.CanSend}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling CanSend for agent {AgentId}", agentId);
            return StatusCode(500, new { error = "Failed to update CanSend" });
        }
    }

    [HttpGet("{agentId:guid}/latest")]
    public async Task<ActionResult<LatestTelemetryDto>> GetLatestTelemetry(Guid agentId)
    {
        try
        {
            var telemetry = await _telemetryService.GetLatestAsync(agentId, HttpContext.RequestAborted);
            if (telemetry == null)
                return NotFound(new { error = "No telemetry found for this agent" });

            return Ok(telemetry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving telemetry for agent {AgentId}", agentId);
            return StatusCode(500, new { error = "Failed to retrieve telemetry" });
        }
    }
}
