using Microsoft.AspNetCore.Mvc;

namespace UX.ControlPlane.Controllers;

[ApiController]
[Route("api/agent")]
public class AgentController : ControllerBase
{
    private readonly AgentService _agentService;
    private readonly ConfigResolverService _configResolver;
    private readonly TelemetryService _telemetryService;
    private readonly ILogger<AgentController> _logger;

    public AgentController(
        AgentService agentService,
        ConfigResolverService configResolver,
        TelemetryService telemetryService,
        ILogger<AgentController> logger)
    {
        _agentService = agentService;
        _configResolver = configResolver;
        _telemetryService = telemetryService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AgentCheckinResponse>> Register([FromBody] AgentRegisterRequest request)
    {
        try
        {
            var agent = await _agentService.RegisterOrUpdateAsync(request, HttpContext.RequestAborted);
            return Ok(await CreateCheckinResponse(agent));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering agent {AgentId}", request.AgentId);
            return StatusCode(500, new { error = "Registration failed" });
        }
    }

    [HttpPost("checkin")]
    public async Task<ActionResult<AgentCheckinResponse>> Checkin([FromBody] AgentCheckinRequest request)
    {
        try
        {
            var agent = await _agentService.UpdateCheckinAsync(request, HttpContext.RequestAborted);
            if (agent == null)
                return NotFound(new { error = "Agent not found. Please register first." });

            return Ok(await CreateCheckinResponse(agent));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during check-in for agent {AgentId}", request.AgentId);
            return StatusCode(500, new { error = "Check-in failed" });
        }
    }

    [HttpPost("telemetry")]
    public async Task<ActionResult<SubmitTelemetryResponse>> SubmitTelemetry([FromBody] SubmitTelemetryRequest request)
    {
        try
        {
            await _telemetryService.StoreLatestAsync(request.AgentId, request, HttpContext.RequestAborted);
            return Ok(new SubmitTelemetryResponse { Accepted = true, Message = "Telemetry stored successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing telemetry for agent {AgentId}", request.AgentId);
            return StatusCode(500, new SubmitTelemetryResponse { Accepted = false, Message = "Failed to store telemetry" });
        }
    }

    private async Task<AgentCheckinResponse> CreateCheckinResponse(Agent agent)
    {
        if (agent.OrgNodeId == null || agent.Status == "PendingAssignment")
        {
            return new AgentCheckinResponse
            {
                CanSend = false,
                EffectiveConfigVersion = 0,
                ConfigHash = "",
                EffectiveConfig = new AgentEffectiveConfig
                {
                    EnableInventory = true,
                    PullConfigEverySeconds = 60,
                    InventoryIntervalMinutes = 60,
                    Ingest = new IngestConfig { Url = "" },
                    WifiPolicy = new WifiPolicy(),
                    Targets = new List<ProbeTarget>()
                },
                NextCheckInSeconds = 60
            };
        }

        var (config, version) = await _configResolver.ResolveEffectiveConfigAsync(agent);
        return new AgentCheckinResponse
        {
            CanSend = agent.CanSend,
            EffectiveConfigVersion = version?.Id ?? 0,
            ConfigHash = version?.Hash ?? "",
            EffectiveConfig = config,
            NextCheckInSeconds = config.PullConfigEverySeconds
        };
    }
}
