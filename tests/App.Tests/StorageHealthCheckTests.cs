using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Organizer.Modules.Shared.Diagnostics.Health;

namespace Organizer.App.Tests;

public class StorageHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenDirectoryIsWritable_ReturnsHealthy()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"organizer-test-{Guid.NewGuid():N}");
        try
        {
            var options = new StorageHealthCheckOptions
            {
                RootPath = tempDir,
                SubDirectories = ["originals", "results"]
            };
            var healthCheck = new StorageHealthCheck(options, NullLogger<StorageHealthCheck>.Instance);
            var context = new HealthCheckContext();

            // Act
            var result = await healthCheck.CheckHealthAsync(context);

            // Assert
            Assert.Equal(HealthStatus.Healthy, result.Status);
            Assert.Contains("accessible and writable", result.Description);
            Assert.True(Directory.Exists(Path.Combine(tempDir, "originals")));
            Assert.True(Directory.Exists(Path.Combine(tempDir, "results")));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task CheckHealthAsync_WhenRootPathIsEmpty_ReturnsUnhealthyWithoutLeakingSecrets()
    {
        // Arrange
        var options = new StorageHealthCheckOptions { RootPath = "" };
        var healthCheck = new StorageHealthCheck(options, NullLogger<StorageHealthCheck>.Instance);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Equal("Storage root path is not configured.", result.Description);
    }
}
