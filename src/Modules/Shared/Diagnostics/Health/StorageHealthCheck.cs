using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Organizer.Modules.Shared.Diagnostics.Health;

public sealed class StorageHealthCheckOptions
{
    public string RootPath { get; set; } = "/storage";
    public string[]? SubDirectories { get; set; }
}

public sealed class StorageHealthCheck : IHealthCheck
{
    private readonly StorageHealthCheckOptions _options;
    private readonly ILogger<StorageHealthCheck> _logger;

    public StorageHealthCheck(
        IOptions<StorageHealthCheckOptions> options,
        ILogger<StorageHealthCheck> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public StorageHealthCheck(
        StorageHealthCheckOptions options,
        ILogger<StorageHealthCheck> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_options.RootPath))
            {
                return HealthCheckResult.Unhealthy("Storage root path is not configured.");
            }

            if (!Directory.Exists(_options.RootPath))
            {
                Directory.CreateDirectory(_options.RootPath);
            }

            if (_options.SubDirectories is { Length: > 0 })
            {
                foreach (var subDir in _options.SubDirectories)
                {
                    var fullPath = Path.Combine(_options.RootPath, subDir);
                    if (!Directory.Exists(fullPath))
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                }
            }

            var probeFile = Path.Combine(_options.RootPath, $".health-probe-{Guid.NewGuid():N}.tmp");
            await File.WriteAllBytesAsync(probeFile, [0x53, 0x54, 0x4F, 0x52], cancellationToken);

            if (File.Exists(probeFile))
            {
                File.Delete(probeFile);
            }

            return HealthCheckResult.Healthy("Storage is accessible and writable.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Storage health check probe failed");
            return HealthCheckResult.Unhealthy("Storage path is inaccessible or not writable.");
        }
    }
}
