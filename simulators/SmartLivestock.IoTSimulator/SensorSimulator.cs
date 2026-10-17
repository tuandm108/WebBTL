using SmartLivestock.Contracts;

namespace SmartLivestock.IoTSimulator;

public static class SensorSimulator
{
    private static readonly Guid BootId = Guid.NewGuid();
    private static readonly string[] EscalatingMetrics = ["temperature", "humidity", "nh3", "h2s"];

    /// <summary>Ticks per escalation cycle: 12 normal, 8 rising, 8 critical, 8 falling (3 minutes at 5 s).</summary>
    public const int EscalationPeriod = 36;

    public static TelemetryMessage Create(string scenario, long sequence, DateTimeOffset now, int barn = 1)
    {
        var kind = EffectiveScenario(scenario, barn).ToUpperInvariant();
        var wave = (decimal)Math.Sin(sequence / 6d + barn);
        var temperature = 27m + wave;
        var humidity = 65m + wave * 2m;
        var nh3 = 2m + wave / 4m;
        var h2s = 0.2m;
        switch (kind)
        {
            case "HIGH_TEMPERATURE": temperature += 8m; break;
            case "HIGH_HUMIDITY": humidity += 20m; break;
            case "HIGH_AMMONIA": nh3 += 10m; break;
            case "HIGH_H2S": h2s += 5m; break;
            case "ESCALATE":
                var metric = EscalatingMetric(barn);
                var envelope = Envelope(sequence, barn);
                decimal Up(decimal value) => value + (DemoAlertProfile.LevelsFor(metric).Critical * 1.1m - value) * envelope;
                switch (metric)
                {
                    case "temperature": temperature = Up(temperature); break;
                    case "humidity": humidity = Up(humidity); break;
                    case "nh3": nh3 = Up(nh3); break;
                    default: h2s = Up(h2s); break;
                }
                break;
            case "NORMAL":
            case "SENSOR_FAILURE": break;
            default: throw new ArgumentException("Unsupported scenario", nameof(scenario));
        }
        var quality = kind == "SENSOR_FAILURE" ? "invalid" : "valid";
        return new TelemetryMessage(1, Guid.NewGuid(), DemoIds.DeviceExternalId(barn), now.ToUniversalTime(),
            sequence, BootId,
            [
                new("temp-01", "temperature", temperature, "C", quality),
                new("humidity-01", "humidity", humidity, "%", quality),
                new("nh3-01", "nh3", nh3, "ppm", quality),
                new("h2s-01", "h2s", h2s, "ppm", quality)
            ]);
    }

    /// <summary>DEMO_ALERTS: every third barn escalates, barn 7 loses its sensors, the rest stay normal.</summary>
    public static string EffectiveScenario(string scenario, int barn) =>
        scenario.Equals("DEMO_ALERTS", StringComparison.OrdinalIgnoreCase)
            ? barn % 3 == 0 ? "ESCALATE" : barn == 7 ? "SENSOR_FAILURE" : "NORMAL"
            : scenario;

    public static string EscalatingMetric(int barn) => EscalatingMetrics[barn % EscalatingMetrics.Length];

    /// <summary>0 = normal, 1 = peak above the critical level. Barns are phase-shifted so alerts do not all fire together.</summary>
    public static decimal Envelope(long sequence, int barn)
    {
        var p = (int)((sequence + barn * 5L) % EscalationPeriod);
        return p < 12 ? 0m : p < 20 ? (p - 11) / 8m : p < 28 ? 1m : (35 - p) / 8m;
    }
}
