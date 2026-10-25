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

var app = builder.Build();

app.UseCors();

app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "Healthy", 
    service = "Organizer.Processing", 
    timestamp = DateTimeOffset.UtcNow 
}));

Console.WriteLine("🚀 [Organizer.Processing] Started successfully on http://localhost:5051");

app.Run();
