namespace SmartLivestock.Contracts;

public static class AlertSeverity
{
    public const string Warning = DemoAlertProfile.Warning;
    public const string Critical = DemoAlertProfile.Critical;
    public const string Fault = "fault";

    public static int Rank(string severity) => severity switch { Critical => 3, Fault => 2, Warning => 1, _ => 0 };
}

public sealed record AlertDto(
    Guid Id, Guid BarnId, string BarnCode, string BarnName, string Type, string? Metric, string Severity,
    string State, DateTimeOffset FirstSeenAt, DateTimeOffset LastSeenAt, DateTimeOffset? ClearedAt,
    decimal? PeakValue, string? Unit, DateTimeOffset? ReadAt = null)
{
    // Filled for history rows only. Current barn occupancy is not a snapshot from FirstSeenAt.
    public int? CurrentAnimalCount { get; init; }
    public IReadOnlyList<SpeciesCountDto>? CurrentSpecies { get; init; }
}

/// <summary>Pushed over SignalR as "alertChanged". Change is raised, escalated or cleared.</summary>
public sealed record AlertChangedDto(string Change, AlertDto Alert);

public sealed record AlertSummaryBarnDto(
    Guid BarnId, string BarnCode, string BarnName, string WorstSeverity, int Count,
    DateTimeOffset FirstSeenAt, DateTimeOffset LastSeenAt, IReadOnlyList<AlertDto> Alerts);

public sealed record AlertSummaryDto(
    DateTimeOffset From, DateTimeOffset To, int Total, int Critical, int Warning, int Fault,
    IReadOnlyList<AlertSummaryBarnDto> Barns);

/// <summary>Open (active) alerts by severity, plus how many alerts of any state are still unread (nav badge).</summary>
public sealed record AlertCountsDto(int Total, int Critical, int Warning, int Fault, int Unread = 0);

/// <summary>Mark alerts read or unread: either explicit <see cref="Ids"/> (max 500) or <see cref="All"/> matching the filters.</summary>
public sealed record AlertReadRequest(
    IReadOnlyList<Guid>? Ids = null, bool Read = true, bool All = false,
    DateTimeOffset? From = null, DateTimeOffset? To = null, Guid? BarnId = null, string? Severity = null, string? State = null);
