using Organizer.Processing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<Worker>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "Healthy", 
    service = "Organizer.Processing", 
    timestamp = DateTimeOffset.UtcNow 
}));

app.Run();
