using System.Net.Http.Json;
using UX.Shared.Common;
using UX.Shared.Contracts;

namespace UX.Agent.Client;

public class ControlPlaneClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ControlPlaneClient> _logger;

    public ControlPlaneClient(HttpClient http, ILogger<ControlPlaneClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<AgentCheckinResponse?> RegisterAsync(AgentRegisterRequest request, CancellationToken cancellationToken)
    {
        var resp = await _http.PostAsJsonAsync("api/agent/register", request, JsonOptions.Default, cancellationToken);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("Register failed: {Status} {Body}", (int)resp.StatusCode, await resp.Content.ReadAsStringAsync(cancellationToken));
            return null;
        }

        return await resp.Content.ReadFromJsonAsync<AgentCheckinResponse>(JsonOptions.Default, cancellationToken);
    }

    public async Task<AgentCheckinResponse?> CheckinAsync(AgentCheckinRequest request, CancellationToken cancellationToken)
    {
        var resp = await _http.PostAsJsonAsync("api/agent/checkin", request, JsonOptions.Default, cancellationToken);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("Checkin failed: {Status} {Body}", (int)resp.StatusCode, await resp.Content.ReadAsStringAsync(cancellationToken));
            return null;
        }

        return await resp.Content.ReadFromJsonAsync<AgentCheckinResponse>(JsonOptions.Default, cancellationToken);
    }

    public async Task<bool> SubmitTelemetryAsync(SubmitTelemetryRequest request, CancellationToken cancellationToken)
    {
        var resp = await _http.PostAsJsonAsync("api/agent/telemetry", request, JsonOptions.Default, cancellationToken);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("Telemetry submit failed: {Status} {Body}", (int)resp.StatusCode, await resp.Content.ReadAsStringAsync(cancellationToken));
            return false;
        }

        return true;
    }
}
