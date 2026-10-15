using SmartLivestock.Contracts;

namespace SmartLivestock.Application;

public static class TelemetryValidator
{
    private static readonly IReadOnlyDictionary<string, string> Units = new Dictionary<string, string>
    {
        ["temperature"] = "C",
        ["humidity"] = "%",
        ["nh3"] = "ppm",
        ["h2s"] = "ppm"
    };

    public static bool TryValidate(TelemetryMessage? message, DateTimeOffset receivedAt, out string error)
    {
        if (message is null || message.SchemaVersion != 1 || message.MessageId == Guid.Empty ||
            message.BootId == Guid.Empty || string.IsNullOrWhiteSpace(message.DeviceId) ||
            message.Sequence < 0 || message.Readings is null || message.Readings.Count is < 1 or > 32)
        {
            error = "Invalid telemetry envelope";
            return false;
        }

        if (message.TimestampUtc.Offset != TimeSpan.Zero ||
            message.TimestampUtc > receivedAt.AddMinutes(5) ||
            message.TimestampUtc < receivedAt.AddDays(-1))
        {
            error = "Invalid measurement time";
            return false;
        }

        foreach (var reading in message.Readings)
        {
            if (string.IsNullOrWhiteSpace(reading.SensorId) ||
                !Units.TryGetValue(reading.Metric, out var unit) || unit != reading.Unit ||
                reading.Quality is not ("valid" or "invalid" or "stale") ||
                (reading.Metric == "humidity" && (reading.Value < 0 || reading.Value > 100)))
            {
                error = "Invalid reading";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }

    public static bool CanDriveControl(ReadingDto reading, DateTimeOffset now) =>
        reading.Quality == "valid" &&
        reading.MeasuredAt <= now &&
        reading.MeasuredAt >= now.AddSeconds(-30);
}
