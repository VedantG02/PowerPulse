using PowerPulse.Api.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var readings = new List<EnergyReading>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        application = "PowerPulse",
        status = "Running",
        timestampUtc = DateTime.UtcNow
    });
});


app.MapPost("/api/readings", (EnergyReading reading) =>
{
    reading.TimestampUtc = DateTime.UtcNow;
    readings.Add(reading);

    return Results.Created(
        $"/api/readings/{reading.DeviceId}",
        reading);
});

app.MapGet("/api/readings", () =>
{
    return Results.Ok(readings
        .OrderByDescending(reading => reading.TimestampUtc));
});
app.Run();
