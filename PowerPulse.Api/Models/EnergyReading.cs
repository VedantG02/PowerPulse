namespace PowerPulse.Api.Models;

public class EnergyReading
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string DeviceId { get; set; } = string.Empty;

    public decimal Voltage { get; set; }

    public decimal Current { get; set; }

    public decimal PowerWatts { get; set; }

    public decimal TemperatureCelsius { get; set; }

    public decimal HumidityPercent { get; set; }

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}