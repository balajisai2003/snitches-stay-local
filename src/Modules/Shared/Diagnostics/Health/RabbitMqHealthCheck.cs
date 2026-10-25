using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Organizer.Modules.Shared.Diagnostics.Health;

public sealed class RabbitMqHealthCheckOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "organizer";
    public string Password { get; set; } = "organizer_secret";
}

public sealed class RabbitMqHealthCheck : IHealthCheck
{
    private readonly RabbitMqHealthCheckOptions _options;
    private readonly ILogger<RabbitMqHealthCheck> _logger;

    public RabbitMqHealthCheck(
        IOptions<RabbitMqHealthCheckOptions> options,
        ILogger<RabbitMqHealthCheck> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public RabbitMqHealthCheck(
        RabbitMqHealthCheckOptions options,
        ILogger<RabbitMqHealthCheck> logger)
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
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(3)
            };

            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            return channel.IsOpen
                ? HealthCheckResult.Healthy("RabbitMQ broker connection is responsive.")
                : HealthCheckResult.Unhealthy("RabbitMQ channel failed to open.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RabbitMQ health check probe failed");
            return HealthCheckResult.Unhealthy("RabbitMQ broker is unreachable.");
        }
    }
}
