using SmartLivestock.Contracts;

namespace SmartLivestock.IoTSimulator;

public static class SensorSimulator
{
    private static readonly Guid BootId = Guid.NewGuid();

    public static TelemetryMessage Create(string scenario, long sequence, DateTimeOffset now)
    {
        var wave = (decimal)Math.Sin(sequence / 6d);
        var temperature = 27m + wave;
        var humidity = 65m + wave * 2m;
        var nh3 = 2m + wave / 4m;
        var h2s = 0.2m;
        switch (scenario.ToUpperInvariant())
        {
            case "HIGH_TEMPERATURE": temperature += 8m; break;
            case "HIGH_HUMIDITY": humidity += 20m; break;
            case "HIGH_AMMONIA": nh3 += 10m; break;
            case "HIGH_H2S": h2s += 5m; break;
            case "NORMAL":
            case "SENSOR_FAILURE": break;
            default: throw new ArgumentException("Unsupported scenario", nameof(scenario));
        }
        var quality = scenario.Equals("SENSOR_FAILURE", StringComparison.OrdinalIgnoreCase)
            ? "invalid" : "valid";
        return new TelemetryMessage(1, Guid.NewGuid(), "edge-001", now.ToUniversalTime(),
            sequence, BootId,
            [
                new("temp-01", "temperature", temperature, "C", quality),
                new("humidity-01", "humidity", humidity, "%", quality),
                new("nh3-01", "nh3", nh3, "ppm", quality),
                new("h2s-01", "h2s", h2s, "ppm", quality)
            ]);
    }
}
