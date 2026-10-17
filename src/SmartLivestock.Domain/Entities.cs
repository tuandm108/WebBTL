namespace SmartLivestock.Domain;

public sealed class Farm
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<Barn> Barns { get; set; } = [];
}

public sealed class Barn
{
    public Guid Id { get; set; }
    public Guid FarmId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public int Capacity { get; set; }
    public bool Monitored { get; set; }
    public Farm Farm { get; set; } = null!;
    public List<Device> Devices { get; set; } = [];
}

public sealed class Device
{
    public Guid Id { get; set; }
    public Guid BarnId { get; set; }
    public required string ExternalId { get; set; }
    public DateTimeOffset? LastSeenAt { get; set; }
    public Barn Barn { get; set; } = null!;
    public List<SensorReading> Readings { get; set; } = [];
}

public sealed class SensorReading
{
    public long Id { get; set; }
    public Guid DeviceId { get; set; }
    public Guid MessageId { get; set; }
    public Guid BootId { get; set; }
    public long Sequence { get; set; }
    public required string SensorId { get; set; }
    public required string Metric { get; set; }
    public decimal Value { get; set; }
    public required string Unit { get; set; }
    public required string Quality { get; set; }
    public DateTimeOffset MeasuredAt { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
    public Device Device { get; set; } = null!;
}

public static class AlertState
{
    public const string Active = "active";
    public const string Cleared = "cleared";
}

public static class AlertType
{
    public const string Threshold = "threshold";
    public const string SensorFault = "sensor_fault";
}

/// <summary>One episode of an abnormal condition in a barn, from first sighting until it clears.</summary>
public sealed class Alert
{
    public Guid Id { get; set; }
    public Guid BarnId { get; set; }
    public required string Type { get; set; }
    public string? Metric { get; set; }
    /// <summary>Worst severity reached during the episode: warning, critical or fault.</summary>
    public required string Severity { get; set; }
    public required string State { get; set; }
    /// <summary>Metric name for threshold alerts, "fault" for sensor faults. Unique among active alerts of a barn.</summary>
    public required string DedupeKey { get; set; }
    public DateTimeOffset FirstSeenAt { get; set; }
    public DateTimeOffset LastSeenAt { get; set; }
    public DateTimeOffset? ClearedAt { get; set; }
    /// <summary>When someone marked this alert as read; reset to null if the alert escalates.</summary>
    public DateTimeOffset? ReadAt { get; set; }
    public decimal? PeakValue { get; set; }
    public string? Unit { get; set; }
    public Barn Barn { get; set; } = null!;
}
