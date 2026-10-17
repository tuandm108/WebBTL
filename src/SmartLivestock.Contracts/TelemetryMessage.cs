using System.Text.Json.Serialization;

namespace SmartLivestock.Contracts;

public sealed record SensorValue(string SensorId, string Metric, decimal Value, string Unit, string Quality);

public sealed record TelemetryMessage(
    int SchemaVersion,
    Guid MessageId,
    string DeviceId,
    DateTimeOffset TimestampUtc,
    long Sequence,
    Guid BootId,
    IReadOnlyList<SensorValue> Readings);

public sealed record ReadingDto(
    Guid BarnId,
    string DeviceId,
    string SensorId,
    string Metric,
    decimal Value,
    string Unit,
    string Quality,
    DateTimeOffset MeasuredAt,
    DateTimeOffset ReceivedAt);

public static class DemoIds
{
    public static readonly Guid Farm = Guid.Parse("79f40d21-f916-42d2-a83a-16d5144b9f01");
    public static readonly Guid Barn = Guid.Parse("c15308f0-224b-4dd2-9aba-764ea7a2f001");
    public static readonly Guid Device = Guid.Parse("e996724d-0a30-461d-93d4-65190594d001");
    public const int SeedBarnCount = 20;

    // Barn 1 / device 1 keep their original IDs; 2..20 follow the same pattern.
    public static Guid BarnId(int n) => Guid.Parse($"c15308f0-224b-4dd2-9aba-764ea7a2f{n:000}");
    public static Guid DeviceId(int n) => Guid.Parse($"e996724d-0a30-461d-93d4-65190594d{n:000}");
    public static string DeviceExternalId(int n) => $"edge-{n:000}";
}
