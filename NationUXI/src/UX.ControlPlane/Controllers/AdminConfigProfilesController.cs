using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UX.ControlPlane.Data.Entities;
using UX.Shared.Contracts;
using SharedJsonOptions = UX.Shared.Common.JsonOptions;

namespace UX.ControlPlane.Controllers;

[ApiController]
[Route("api/admin/configprofiles")]
public class AdminConfigProfilesController : ControllerBase
{
    private readonly UxDbContext _dbContext;
    private readonly AuditService _auditService;
    private readonly ILogger<AdminConfigProfilesController> _logger;

    public AdminConfigProfilesController(UxDbContext dbContext, AuditService auditService, ILogger<AdminConfigProfilesController> logger)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ConfigProfileDto>> CreateProfile([FromBody] CreateConfigProfileRequest request)
    {
        try
        {
            var errors = ValidateConfigJson(request.ConfigJson);
            if (errors.Any())
                return BadRequest(new { errors });

            var profile = new ConfigProfile
            {
                Name = request.Name,
                Description = request.Description,
                ConfigJson = request.ConfigJson,
                CreatedUtc = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name ?? "System"
            };

            _dbContext.ConfigProfiles.Add(profile);
            await _dbContext.SaveChangesAsync();

            await _auditService.LogAsync(
                "ConfigProfileCreated",
                "ConfigProfile",
                profile.Id.ToString(),
                null,
                new { profile.Name, profile.Description },
                HttpContext.RequestAborted);

            return CreatedAtAction(nameof(GetProfile), new { id = profile.Id }, MapToDto(profile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating config profile");
            return StatusCode(500, new { error = "Failed to create configuration profile" });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] CreateConfigProfileRequest request)
    {
        try
        {
            var profile = await _dbContext.ConfigProfiles.FindAsync(id);
            if (profile == null)
                return NotFound(new { error = "Profile not found" });

            var errors = ValidateConfigJson(request.ConfigJson);
            if (errors.Any())
                return BadRequest(new { errors });

            var before = new { profile.Name, profile.Description, profile.ConfigJson };

            profile.Name = request.Name;
            profile.Description = request.Description;
            profile.ConfigJson = request.ConfigJson;

            await _dbContext.SaveChangesAsync();

            await _auditService.LogAsync(
                "ConfigProfileUpdated",
                "ConfigProfile",
                profile.Id.ToString(),
                before,
                new { profile.Name, profile.Description },
                HttpContext.RequestAborted);

            return Ok(new { message = "Profile updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating config profile {Id}", id);
            return StatusCode(500, new { error = "Failed to update configuration profile" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ConfigProfileDto>>> ListProfiles()
    {
        try
        {
            var profiles = await _dbContext.ConfigProfiles
                .OrderByDescending(p => p.CreatedUtc)
                .ToListAsync();

            return Ok(profiles.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing config profiles");
            return StatusCode(500, new { error = "Failed to retrieve configuration profiles" });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConfigProfileDto>> GetProfile(Guid id)
    {
        try
        {
            var profile = await _dbContext.ConfigProfiles.FindAsync(id);
            if (profile == null)
                return NotFound(new { error = "Profile not found" });

            return Ok(MapToDto(profile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving config profile {Id}", id);
            return StatusCode(500, new { error = "Failed to retrieve configuration profile" });
        }
    }

    [HttpPost("validate")]
    public IActionResult ValidateConfig([FromBody] string configJson)
    {
        var errors = ValidateConfigJson(configJson);
        return Ok(new ValidateConfigResultDto { IsValid = !errors.Any(), Errors = errors });
    }

    private List<string> ValidateConfigJson(string json)
    {
        var errors = new List<string>();
        try
        {
            var config = JsonSerializer.Deserialize<AgentEffectiveConfig>(json, SharedJsonOptions.Default);
            if (config == null)
            {
                errors.Add("Config cannot be null");
                return errors;
            }

            var validation = config.Validate();
            if (!validation.IsValid)
                errors.AddRange(validation.Errors);
        }
        catch (JsonException ex)
        {
            errors.Add($"Invalid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            errors.Add($"Error parsing config: {ex.Message}");
        }

        return errors;
    }

    private static ConfigProfileDto MapToDto(ConfigProfile profile) => new()
    {
        Id = profile.Id,
        Name = profile.Name,
        Description = profile.Description,
        ConfigJson = profile.ConfigJson,
        CreatedUtc = profile.CreatedUtc,
        CreatedBy = profile.CreatedBy
    };
}
