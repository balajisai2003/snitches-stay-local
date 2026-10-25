using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Organizer.App.Diagnostics;

public sealed class PostgresHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly ILogger<PostgresHealthCheck> _logger;

    public PostgresHealthCheck(IConfiguration configuration, ILogger<PostgresHealthCheck> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            return HealthCheckResult.Unhealthy("PostgreSQL connection string is not configured.");
        }

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(3));

            await conn.OpenAsync(timeoutCts.Token);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1;";
            var result = await cmd.ExecuteScalarAsync(timeoutCts.Token);

            if (result is int i && i == 1)
            {
                return HealthCheckResult.Healthy("PostgreSQL server is responsive.");
            }

            return HealthCheckResult.Degraded("PostgreSQL query did not return expected response.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PostgreSQL health check probe failed");
            // Security Rule #6: Never leak connection string, credentials, or internal details
            return HealthCheckResult.Unhealthy("PostgreSQL database is unreachable.");
        }
    }
}
