using System.Net.Http.Json;

const string apiBaseUrl = "https://localhost:7292";
const string deviceId = "esp32-lab-01";

using var client = new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
};

var random = new Random();

Console.WriteLine("PowerPulse device simulator started.");
Console.WriteLine($"Sending readings to {apiBaseUrl} as {deviceId}.");

while (true)
{
    var voltage = 5.0;
    var current = Math.Round(0.10 + (random.NextDouble() * 0.60), 2);
    var powerWatts = Math.Round(voltage * current, 2);
    var temperatureCelsius = Math.Round(24 + (random.NextDouble() * 6), 1);
    var humidityPercent = Math.Round(45 + (random.NextDouble() * 20), 1);

    var reading = new
    {
        deviceId,
        voltage,
        current,
        powerWatts,
        temperatureCelsius,
        humidityPercent
    };

    var response = await client.PostAsJsonAsync("/api/readings", reading);

    Console.WriteLine(
        $"{DateTime.Now:T} | " +
        $"{powerWatts} W | " +
        $"HTTP {(int)response.StatusCode}");

    await Task.Delay(TimeSpan.FromSeconds(5));
}