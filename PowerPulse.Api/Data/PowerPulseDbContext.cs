using Microsoft.EntityFrameworkCore;
using PowerPulse.Api.Models;

namespace PowerPulse.Api.Data;

public class PowerPulseDbContext : DbContext
{
    public PowerPulseDbContext(DbContextOptions<PowerPulseDbContext> options)
        : base(options)
    {
    }

    public DbSet<EnergyReading> EnergyReadings => Set<EnergyReading>();
}