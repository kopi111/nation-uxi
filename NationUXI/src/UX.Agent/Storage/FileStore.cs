using System.Text.Json;
using UX.Shared.Common;

namespace UX.Agent.Storage;

public class FileStore
{
    private readonly ILogger<FileStore> _logger;

    public FileStore(ILogger<FileStore> logger)
    {
        _logger = logger;
    }

    public async Task<T?> ReadAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(path))
                return default;

            var json = await File.ReadAllTextAsync(path, cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
                return default;

            return JsonSerializer.Deserialize<T>(json, JsonOptions.Default);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read {Path}", path);
            return default;
        }
    }

    public async Task WriteAsync<T>(string path, T value, CancellationToken cancellationToken = default)
    {
        try
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(value, JsonOptions.Indented);
            await File.WriteAllTextAsync(path, json, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write {Path}", path);
        }
    }
}
