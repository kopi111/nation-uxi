using System.Text.Json;
using UX.Shared.Common;

namespace UX.ControlPlane.Services;

public class AuditService
{
    private readonly UxDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        UxDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditService> logger)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(
        string action,
        string entity,
        string entityId,
        object? before = null,
        object? after = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var actor = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";

            var auditLog = new AuditLog
            {
                Actor = actor,
                Action = action,
                Entity = entity,
                EntityId = entityId,
                Utc = DateTime.UtcNow,
                BeforeJson = before != null ? JsonSerializer.Serialize(before, JsonOptions.Default) : null,
                AfterJson = after != null ? JsonSerializer.Serialize(after, JsonOptions.Default) : null
            };

            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit log for {Action} on {Entity}/{EntityId}",
                action, entity, entityId);
        }
    }
}
