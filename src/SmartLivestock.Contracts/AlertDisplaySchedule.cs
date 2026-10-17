namespace SmartLivestock.Contracts;

/// <summary>How and when the alert digest dialog may interrupt the user. Stored per browser.</summary>
public sealed record AlertDisplaySettings(
    string Mode = AlertDisplaySchedule.Daily,
    string Time = "23:59",
    int Minutes = 30,
    bool OnOpen = false);

/// <summary>
/// Pure scheduling rules for the alert digest dialog, kept free of UI and clocks so they can be unit tested.
/// "now" and "lastRun" must carry the same local offset: the daily time is a wall-clock time.
/// </summary>
public static class AlertDisplaySchedule
{
    public const string Daily = "daily";
    public const string Interval = "interval";
    public const string Immediate = "immediate";
    public const string Off = "off";

    public static readonly int[] IntervalChoices = [15, 30, 60, 120, 240];

    /// <summary>Repairs anything unreadable (bad JSON, hand-edited storage) back to safe defaults.</summary>
    public static AlertDisplaySettings Normalize(AlertDisplaySettings? settings)
    {
        settings ??= new AlertDisplaySettings();
        var mode = settings.Mode is Daily or Interval or Immediate or Off ? settings.Mode : Daily;
        var time = TimeOnly.TryParseExact(settings.Time, "HH:mm", out _) ? settings.Time : "23:59";
        return new AlertDisplaySettings(mode, time, Math.Clamp(settings.Minutes, 1, 1440), settings.OnOpen);
    }

    /// <summary>
    /// True when a scheduled moment has passed since the last run. With no previous run the schedule has not
    /// started yet (the caller records "now"), so opening the app for the first time never fires a flood.
    /// A daily slot that passed while the app was closed fires once on the next open.
    /// </summary>
    public static bool IsDue(AlertDisplaySettings settings, DateTimeOffset now, DateTimeOffset? lastRun)
    {
        if (lastRun is not { } last) return false;
        return settings.Mode switch
        {
            Daily => last < LastOccurrence(settings, now),
            Interval => now - last >= TimeSpan.FromMinutes(settings.Minutes),
            _ => false
        };
    }

    /// <summary>The next moment the digest will show, for display; null when the mode never schedules.</summary>
    public static DateTimeOffset? NextDue(AlertDisplaySettings settings, DateTimeOffset now, DateTimeOffset? lastRun) =>
        settings.Mode switch
        {
            Daily => lastRun is { } last && last >= LastOccurrence(settings, now)
                ? LastOccurrence(settings, now).AddDays(1)
                : now,
            Interval => lastRun is { } l ? l.AddMinutes(settings.Minutes) : now.AddMinutes(settings.Minutes),
            _ => null
        };

    /// <summary>Most recent HH:mm slot at or before <paramref name="now"/>.</summary>
    private static DateTimeOffset LastOccurrence(AlertDisplaySettings settings, DateTimeOffset now)
    {
        var time = TimeOnly.ParseExact(settings.Time, "HH:mm");
        var slot = new DateTimeOffset(now.Year, now.Month, now.Day, time.Hour, time.Minute, 0, now.Offset);
        return slot > now ? slot.AddDays(-1) : slot;
    }
}
