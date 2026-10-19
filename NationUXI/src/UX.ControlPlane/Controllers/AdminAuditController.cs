using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace UX.ControlPlane.Controllers;

[ApiController]
[Route("api/admin/audit")]
public class AdminAuditController : ControllerBase
{
    private readonly UxDbContext _dbContext;

    public AdminAuditController(UxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("latest")]
    public async Task<ActionResult<List<object>>> Latest([FromQuery] int take = 50)
    {
        take = Math.Clamp(take, 1, 200);

        var rows = await _dbContext.AuditLogs
            .OrderByDescending(a => a.Utc)
            .Take(take)
            .ToListAsync();

        var shaped = rows.Select(r => new
        {
            r.Id,
            r.Utc,
            r.Actor,
            r.Action,
            r.Entity,
            r.EntityId,
            r.BeforeJson,
            r.AfterJson
        });

        return Ok(shaped);
    }
}
