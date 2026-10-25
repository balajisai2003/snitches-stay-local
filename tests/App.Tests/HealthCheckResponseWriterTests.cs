using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Organizer.Modules.Shared.Diagnostics.Health;

namespace Organizer.App.Tests;

public class HealthCheckResponseWriterTests
{
    [Fact]
    public async Task WriteResponse_OutputsValidJsonWithStableComponents()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var entries = new Dictionary<string, HealthReportEntry>
        {
            ["database"] = new(HealthStatus.Healthy, "Database ok", TimeSpan.FromMilliseconds(5), null, null),
            ["rabbitmq"] = new(HealthStatus.Healthy, "Broker ok", TimeSpan.FromMilliseconds(3), null, null),
            ["storage"] = new(HealthStatus.Healthy, "Storage ok", TimeSpan.FromMilliseconds(1), null, null)
        };

        var report = new HealthReport(entries, TimeSpan.FromMilliseconds(9));

        // Act
        await HealthCheckResponseWriter.WriteResponse(context, report);

        // Assert
        Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);

        responseBody.Seek(0, SeekOrigin.Begin);
        using var document = await JsonDocument.ParseAsync(responseBody);
        var root = document.RootElement;

        Assert.Equal("Healthy", root.GetProperty("status").GetString());
        Assert.True(root.TryGetProperty("timestamp", out _));
        Assert.True(root.TryGetProperty("totalDurationMs", out _));

        var components = root.GetProperty("components");
        Assert.True(components.TryGetProperty("database", out var dbProp));
        Assert.Equal("Healthy", dbProp.GetProperty("status").GetString());
        Assert.Equal("Database ok", dbProp.GetProperty("description").GetString());

        Assert.True(components.TryGetProperty("rabbitmq", out var mqProp));
        Assert.Equal("Healthy", mqProp.GetProperty("status").GetString());

        Assert.True(components.TryGetProperty("storage", out var storageProp));
        Assert.Equal("Healthy", storageProp.GetProperty("status").GetString());
    }
}
