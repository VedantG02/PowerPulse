using Microsoft.EntityFrameworkCore;
using PowerPulse.Api.Data;
using PowerPulse.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<PowerPulseDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("PowerPulseDatabase")));

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<PowerPulseDbContext>();
    database.Database.EnsureCreated();
}

app.UseHttpsRedirection();

app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        application = "PowerPulse",
        status = "Running",
        timestampUtc = DateTime.UtcNow
    });
});

app.MapPost("/api/readings", async (
    EnergyReading reading,
    PowerPulseDbContext database) =>
{
    reading.Id = Guid.NewGuid();
    reading.TimestampUtc = DateTime.UtcNow;

    database.EnergyReadings.Add(reading);
    await database.SaveChangesAsync();

    return Results.Created(
        $"/api/readings/{reading.Id}",
        reading);
});

app.MapGet("/api/readings", async (PowerPulseDbContext database) =>
{
    var readings = await database.EnergyReadings
        .OrderByDescending(reading => reading.TimestampUtc)
        .ToListAsync();

    return Results.Ok(readings);
});

app.Run();