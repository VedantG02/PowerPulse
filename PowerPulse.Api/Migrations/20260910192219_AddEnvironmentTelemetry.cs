using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerPulse.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEnvironmentTelemetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HumidityPercent",
                table: "EnergyReadings",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TemperatureCelsius",
                table: "EnergyReadings",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HumidityPercent",
                table: "EnergyReadings");

            migrationBuilder.DropColumn(
                name: "TemperatureCelsius",
                table: "EnergyReadings");
        }
    }
}
