using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Organizer.Modules.Shared.Diagnostics.Health;

namespace Organizer.Processing.Tests;

public class ProcessingHealthCheckTests
{
    [Fact]
    public async Task StorageHealthCheck_WithResultsAndOriginalsSubdirs_EnsuresBothExist()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"processing-test-{Guid.NewGuid():N}");
        try
        {
            var options = new StorageHealthCheckOptions
            {
                RootPath = tempDir,
                SubDirectories = ["originals", "results"]
            };
            var healthCheck = new StorageHealthCheck(options, NullLogger<StorageHealthCheck>.Instance);

            // Act
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            // Assert
            Assert.Equal(HealthStatus.Healthy, result.Status);
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
}
