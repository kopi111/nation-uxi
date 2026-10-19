using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UX.Shared.Common;

namespace UX.ControlPlane.Services;

public class TelemetryService
{
    private readonly UxDbContext _dbContext;
    private readonly ILogger<TelemetryService> _logger;

    public TelemetryService(UxDbContext dbContext, ILogger<TelemetryService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task StoreLatestAsync(Guid agentId, SubmitTelemetryRequest request, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            request.CollectedUtc,
            request.Metrics,
            request.Inventory,
            request.NetworkHints
        };

        var json = JsonSerializer.Serialize(payload, JsonOptions.Default);

        var latest = await _dbContext.AgentTelemetryLatest.FirstOrDefaultAsync(t => t.AgentId == agentId, cancellationToken);
        if (latest == null)
        {
            latest = new AgentTelemetryLatest { AgentId = agentId };
            _dbContext.AgentTelemetryLatest.Add(latest);
        }

        latest.LastPayloadReceivedUtc = DateTime.UtcNow;
        latest.LastPayloadJson = json;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<LatestTelemetryDto?> GetLatestAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        var latest = await _dbContext.AgentTelemetryLatest.FirstOrDefaultAsync(t => t.AgentId == agentId, cancellationToken);
        if (latest == null) return null;

        return new LatestTelemetryDto
        {
            AgentId = latest.AgentId,
            LastPayloadReceivedUtc = latest.LastPayloadReceivedUtc,
            LastPayloadJson = latest.LastPayloadJson
        };
    }
}
