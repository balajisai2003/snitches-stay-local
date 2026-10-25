using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Organizer.App.Diagnostics;
using Organizer.Modules.Shared.Diagnostics.Health;

namespace Organizer.App.Tests;

public class SecurityHealthCheckTests
{
    [Fact]
    public async Task PostgresHealthCheck_WhenConnectionFails_NeverLeaksPasswordOrConnectionString()
    {
        // Arrange
        const string secretPassword = "SuperSecret_P@ssw0rd!_12345";
        var inMemoryConfig = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = $"Host=127.0.0.1;Port=59999;Database=fake_db;Username=fake_user;Password={secretPassword};Timeout=1"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        var healthCheck = new PostgresHealthCheck(configuration, NullLogger<PostgresHealthCheck>.Instance);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert: Component must be Unhealthy and MUST NOT leak secrets
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.DoesNotContain(secretPassword, result.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("fake_user", result.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("fake_db", result.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("PostgreSQL database is unreachable.", result.Description);
    }

    [Fact]
    public async Task RabbitMqHealthCheck_WhenBrokerIsUnreachable_NeverLeaksPasswordOrUser()
    {
        // Arrange
        const string secretPassword = "BrokerSecret_Password!_98765";
        var options = new RabbitMqHealthCheckOptions
        {
            HostName = "127.0.0.1",
            Port = 59998,
            UserName = "secret_broker_user",
            Password = secretPassword
        };

        var healthCheck = new RabbitMqHealthCheck(options, NullLogger<RabbitMqHealthCheck>.Instance);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert: Component must be Unhealthy and MUST NOT leak secrets
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.DoesNotContain(secretPassword, result.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret_broker_user", result.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("RabbitMQ broker is unreachable.", result.Description);
    }
}
