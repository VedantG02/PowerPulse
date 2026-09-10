using Microsoft.EntityFrameworkCore;
using PowerPulse.Api.Data;
using PowerPulse.Api.Models;

var builder = WebApplication.CreateBuilder(args);
var powerAlertThresholdWatts = builder.Configuration.GetValue<decimal>("PowerPulse:PowerAlertThresholdWatts");
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

app.MapGet("/api/summary", async (PowerPulseDbContext database) =>
{
    var today = DateTime.UtcNow.Date;

    var readings = await database.EnergyReadings
        .Where(reading => reading.TimestampUtc >= today)
        .OrderBy(reading => reading.TimestampUtc)
        .ToListAsync();

    if (readings.Count == 0)
    {
        return Results.NotFound(new
        {
            message = "No readings have been received today."
        });
    }

    var latest = readings[^1];

    decimal estimatedEnergyWh = 0;

    for (var index = 1; index < readings.Count; index++)
    {
        var previous = readings[index - 1];
        var current = readings[index];

        var hoursBetweenReadings =
            (decimal)(current.TimestampUtc - previous.TimestampUtc).TotalHours;

        var averagePowerBetweenReadings =
            (previous.PowerWatts + current.PowerWatts) / 2;

        estimatedEnergyWh += averagePowerBetweenReadings * hoursBetweenReadings;
    }

    return Results.Ok(new
    {
        deviceId = latest.DeviceId,
        latestPowerWatts = latest.PowerWatts,
        averagePowerWatts = Math.Round(readings.Average(reading => reading.PowerWatts), 2),
        peakPowerWatts = readings.Max(reading => reading.PowerWatts),
        estimatedEnergyWh = Math.Round(estimatedEnergyWh, 3),
        powerAlertThresholdWatts,
        isPowerAlert = latest.PowerWatts >= powerAlertThresholdWatts,
        readingsToday = readings.Count
    });
});

app.Run();