using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace UX.ControlPlane.Controllers;

[ApiController]
[Route("api/admin/configassignments")]
public class AdminConfigAssignmentsController : ControllerBase
{
    private readonly UxDbContext _dbContext;
    private readonly AuditService _auditService;
    private readonly ILogger<AdminConfigAssignmentsController> _logger;

    public AdminConfigAssignmentsController(UxDbContext dbContext, AuditService auditService, ILogger<AdminConfigAssignmentsController> logger)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ConfigAssignmentDto>> CreateAssignment([FromBody] CreateConfigAssignmentRequest request)
    {
        try
        {
            var scopeType = NormalizeScopeType(request.ScopeType);

            if (!await ValidateScopeExists(scopeType, request.ScopeId))
                return BadRequest(new { error = $"Invalid scope: {scopeType}/{request.ScopeId}" });

            var profile = await _dbContext.ConfigProfiles.FindAsync(request.ConfigProfileId);
            if (profile == null)
                return BadRequest(new { error = "Config profile not found" });

            var assignment = new ConfigAssignment
            {
                ScopeType = scopeType,
                ScopeId = request.ScopeId,
                ConfigProfileId = request.ConfigProfileId,
                Priority = request.Priority,
                Enabled = true,
                EffectiveFromUtc = request.EffectiveFromUtc
            };

            _dbContext.ConfigAssignments.Add(assignment);
            await _dbContext.SaveChangesAsync();

            await _auditService.LogAsync(
                "ConfigAssignmentCreated",
                "ConfigAssignment",
                assignment.Id.ToString(),
                null,
                new { assignment.ScopeType, assignment.ScopeId, assignment.ConfigProfileId, assignment.Priority },
                HttpContext.RequestAborted);

            return CreatedAtAction(nameof(GetAssignment), new { id = assignment.Id }, await MapToDto(assignment));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating config assignment");
            return StatusCode(500, new { error = "Failed to create configuration assignment" });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConfigAssignmentDto>> GetAssignment(Guid id)
    {
        var assignment = await _dbContext.ConfigAssignments
            .Include(a => a.ConfigProfile)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assignment == null)
            return NotFound(new { error = "Assignment not found" });

        return Ok(await MapToDto(assignment));
    }

    [HttpGet("scope/{scopeType}/{scopeId:guid}")]
    public async Task<ActionResult<List<ConfigAssignmentDto>>> GetScopeAssignments(string scopeType, Guid scopeId)
    {
        try
        {
            var normalized = NormalizeScopeType(scopeType);

            var assignments = await _dbContext.ConfigAssignments
                .Include(a => a.ConfigProfile)
                .Where(a => a.ScopeType == normalized && a.ScopeId == scopeId)
                .OrderBy(a => a.Priority)
                .ToListAsync();

            var dtos = new List<ConfigAssignmentDto>();
            foreach (var a in assignments)
                dtos.Add(await MapToDto(a));

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignments for scope {ScopeType}/{ScopeId}", scopeType, scopeId);
            return StatusCode(500, new { error = "Failed to retrieve assignments" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ConfigAssignmentDto>>> ListAll()
    {
        var assignments = await _dbContext.ConfigAssignments
            .Include(a => a.ConfigProfile)
            .OrderBy(a => a.ScopeType)
            .ThenBy(a => a.Priority)
            .ToListAsync();

        var dtos = new List<ConfigAssignmentDto>();
        foreach (var a in assignments)
            dtos.Add(await MapToDto(a));

        return Ok(dtos);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var assignment = await _dbContext.ConfigAssignments.FindAsync(id);
        if (assignment == null)
            return NotFound(new { error = "Assignment not found" });

        _dbContext.ConfigAssignments.Remove(assignment);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync(
            "ConfigAssignmentDeleted",
            "ConfigAssignment",
            id.ToString(),
            new { assignment.ScopeType, assignment.ScopeId, assignment.ConfigProfileId, assignment.Priority },
            null,
            HttpContext.RequestAborted);

        return NoContent();
    }

    private string NormalizeScopeType(string scopeType) => scopeType?.ToLower() switch
    {
        "agent" => "Agent",
        "orgnode" => "OrgNode",
        _ => scopeType
    };

    private async Task<bool> ValidateScopeExists(string scopeType, Guid scopeId) => scopeType switch
    {
        "Agent" => await _dbContext.Agents.AnyAsync(a => a.AgentId == scopeId),
        "OrgNode" => await _dbContext.OrgNodes.AnyAsync(n => n.Id == scopeId),
        _ => false
    };

    private async Task<string> GetScopeName(string scopeType, Guid scopeId) => scopeType switch
    {
        "Agent" => (await _dbContext.Agents.FindAsync(scopeId))?.MachineName ?? "Unknown Agent",
        "OrgNode" => (await _dbContext.OrgNodes.FindAsync(scopeId))?.Name ?? "Unknown OrgNode",
        _ => "Unknown"
    };

    private async Task<ConfigAssignmentDto> MapToDto(ConfigAssignment assignment)
    {
        var scopeName = await GetScopeName(assignment.ScopeType, assignment.ScopeId);
        return new ConfigAssignmentDto
        {
            Id = assignment.Id,
            ScopeType = assignment.ScopeType,
            ScopeId = assignment.ScopeId,
            ScopeName = scopeName,
            ConfigProfileId = assignment.ConfigProfileId,
            ConfigProfileName = assignment.ConfigProfile?.Name ?? "",
            Priority = assignment.Priority,
            Enabled = assignment.Enabled,
            EffectiveFromUtc = assignment.EffectiveFromUtc
        };
    }
}
