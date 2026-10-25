var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors();

app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "Healthy", 
    service = "Organizer.App", 
    timestamp = DateTimeOffset.UtcNow 
}));

Console.WriteLine($"🚀 [Organizer.App] Running in {app.Environment.EnvironmentName} mode");

app.Run();
