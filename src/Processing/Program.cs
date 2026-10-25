using Organizer.Modules.Shared.Diagnostics.Health;
using Organizer.Processing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<Worker>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.Configure<StorageHealthCheckOptions>(options =>
{
    options.RootPath = builder.Configuration["Storage:RootPath"] ?? "/storage";
    options.SubDirectories = ["originals", "results"];
});

builder.Services.Configure<RabbitMqHealthCheckOptions>(options =>
{
    options.HostName = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
    if (int.TryParse(builder.Configuration["RabbitMQ:Port"], out var port))
    {
        options.Port = port;
    }
    options.UserName = builder.Configuration["RabbitMQ:Username"] ?? "organizer";
    options.Password = builder.Configuration["RabbitMQ:Password"] ?? "organizer_secret";
});

builder.Services.AddHealthChecks()
    .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: ["ready", "messaging"])
    .AddCheck<StorageHealthCheck>("storage", tags: ["ready", "storage"]);

var app = builder.Build();

app.UseCors();

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

Console.WriteLine($"🚀 [Organizer.Processing] Running in {app.Environment.EnvironmentName} mode");

app.Run();
