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
