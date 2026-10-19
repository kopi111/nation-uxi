using Microsoft.AspNetCore.Mvc;

namespace UX.ControlPlane.Controllers;

[ApiController]
[Route("api/admin/orgnodes")]
public class AdminOrgNodesController : ControllerBase
{
    private readonly OrgTreeService _orgTreeService;
    private readonly ILogger<AdminOrgNodesController> _logger;

    public AdminOrgNodesController(OrgTreeService orgTreeService, ILogger<AdminOrgNodesController> logger)
    {
        _orgTreeService = orgTreeService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<OrgNodeDto>> CreateNode([FromBody] CreateOrgNodeRequest request)
    {
        try
        {
            var node = await _orgTreeService.CreateAsync(request, HttpContext.RequestAborted);
            var dto = await _orgTreeService.GetNodeAsync(node.Id, HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetNode), new { id = node.Id }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating org node");
            return StatusCode(500, new { error = "Failed to create organization node" });
        }
    }

    [HttpGet("tree")]
    public async Task<ActionResult<List<OrgNodeDto>>> GetTree()
    {
        try
        {
            var tree = await _orgTreeService.GetTreeAsync(HttpContext.RequestAborted);
            return Ok(tree);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving org tree");
            return StatusCode(500, new { error = "Failed to retrieve organization tree" });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrgNodeDto>> GetNode(Guid id)
    {
        try
        {
            var node = await _orgTreeService.GetNodeAsync(id, HttpContext.RequestAborted);
            if (node == null)
                return NotFound(new { error = "Organization node not found" });

            return Ok(node);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving org node {Id}", id);
            return StatusCode(500, new { error = "Failed to retrieve organization node" });
        }
    }
}
