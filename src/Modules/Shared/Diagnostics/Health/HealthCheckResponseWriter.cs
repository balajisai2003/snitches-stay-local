using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Organizer.Modules.Shared.Diagnostics.Health;

public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var components = new Dictionary<string, object>();
        foreach (var (key, entry) in report.Entries)
        {
            components[key] = new
            {
                status = entry.Status.ToString(),
                durationMs = Math.Round(entry.Duration.TotalMilliseconds, 2),
                description = entry.Description ?? string.Empty
            };
        }

        var serviceName = context.RequestServices
            .GetService<IHostEnvironment>()?.ApplicationName ?? "Organizer";

        var response = new
        {
            status = report.Status.ToString(),
            service = serviceName,
            timestamp = DateTime.UtcNow.ToString("o"),
            totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 2),
            components
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
