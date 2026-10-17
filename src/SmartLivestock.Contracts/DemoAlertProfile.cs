namespace SmartLivestock.Contracts;

/// <summary>
/// Synthetic alert levels for the simulator and UI demo only. Decision D-003: no universal livestock
/// threshold is seeded; real limits depend on species, age and housing and must be configured per
/// profile after review (F13). Severity here is instantaneous, with no deadband or on/off delay.
/// </summary>
public static class DemoAlertProfile
{
    public const string Normal = "normal";
    public const string Warning = "warning";
    public const string Critical = "critical";

    private static readonly Dictionary<string, (decimal Warning, decimal Critical)> Levels = new()
    {
        ["temperature"] = (30m, 35m),
        ["humidity"] = (80m, 90m),
        ["nh3"] = (10m, 25m),
        ["h2s"] = (5m, 10m)
    };

    public static (decimal Warning, decimal Critical) LevelsFor(string metric) => Levels[metric];

    public static string Severity(string metric, decimal value) =>
        !Levels.TryGetValue(metric, out var level) ? Normal
        : value >= level.Critical ? Critical
        : value >= level.Warning ? Warning
        : Normal;

    /// <summary>
    /// Severity with hysteresis: an open alert only steps down once the value is a deadband (5 % of the
    /// warning level, demo value) below the level that raised it, so values hovering at a limit do not chatter.
    /// </summary>
    public static string Next(string metric, decimal value, string? previous)
    {
        if (!Levels.TryGetValue(metric, out var level)) return Normal;
        var band = level.Warning * 0.05m;
        var raw = Severity(metric, value);
        if (previous == Critical && value >= level.Critical - band) return Critical;
        if (previous is Warning or Critical && raw == Normal && value >= level.Warning - band) return Warning;
        return raw;
    }
}
