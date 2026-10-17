using SmartLivestock.Contracts;

namespace SmartLivestock.Web.Components;

/// <summary>Display helpers shared by the monitoring pages.</summary>
public static class BarnStatus
{
    public static readonly string[] Metrics = ["temperature", "humidity", "nh3", "h2s"];

    // Same 30 s freshness window the API uses before a reading may drive control (TelemetryValidator.CanDriveControl).
    private static readonly TimeSpan Fresh = TimeSpan.FromSeconds(30);

    public static string Label(string metric) => metric switch
    {
        "temperature" => "Nhiệt độ",
        "humidity" => "Độ ẩm",
        "nh3" => "NH₃",
        "h2s" => "H₂S",
        _ => metric
    };

    /// <summary>Severity of one reading against the demo profile; invalid or stale data never counts as an alert level.</summary>
    public static string Severity(ReadingDto reading) =>
        reading.Quality == "valid" ? DemoAlertProfile.Severity(reading.Metric, reading.Value) : DemoAlertProfile.Normal;

    /// <summary>Chip for one metric card.</summary>
    public static (string Css, string Text) Of(ReadingDto reading) => reading.Quality switch
    {
        "valid" => Severity(reading) switch
        {
            DemoAlertProfile.Critical => ("danger", "Nguy hiểm"),
            DemoAlertProfile.Warning => ("warn", "Cảnh báo"),
            _ => ("ok", "Bình thường")
        },
        "stale" => ("warn", "Dữ liệu cũ"),
        _ => ("danger", "Cảm biến lỗi")
    };

    /// <summary>Overall barn status; the worst condition wins.</summary>
    public static (string Css, string Text) Of(IReadOnlyCollection<ReadingDto> readings)
    {
        if (readings.Count == 0 || DateTimeOffset.UtcNow - readings.Max(x => x.MeasuredAt) > Fresh)
            return ("warn", "Mất tín hiệu");
        if (readings.Any(x => x.Quality != "valid")) return ("danger", "Cảm biến lỗi");
        if (readings.Any(x => Severity(x) == DemoAlertProfile.Critical)) return ("danger", "Nguy hiểm");
        if (readings.Any(x => Severity(x) == DemoAlertProfile.Warning)) return ("warn", "Cảnh báo");
        return ("ok", "Trực tuyến");
    }

    public static string Age(DateTimeOffset measuredAt)
    {
        var age = DateTimeOffset.UtcNow - measuredAt;
        return age.TotalSeconds < 5 ? "vừa xong"
            : age.TotalSeconds < 60 ? $"{(int)age.TotalSeconds} giây trước"
            : age.TotalMinutes < 60 ? $"{(int)age.TotalMinutes} phút trước"
            : measuredAt.ToLocalTime().ToString("HH:mm dd/MM");
    }
}
