var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "Healthy", 
    service = "Organizer.App", 
    timestamp = DateTimeOffset.UtcNow 
}));

app.Run();
