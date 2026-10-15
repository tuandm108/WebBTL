using SmartLivestock.Application;
using SmartLivestock.Contracts;

namespace SmartLivestock.UnitTests;

public sealed class TelemetryValidatorTests
{
    [Fact]
    public void ValidTelemetryAccepted()
    {
        var now = DateTimeOffset.UtcNow;
        var message = new TelemetryMessage(1, Guid.NewGuid(), "edge-001", now, 1,
            Guid.NewGuid(), [new("temp-01", "temperature", 28m, "C", "valid")]);
        Assert.True(TelemetryValidator.TryValidate(message, now, out _));
    }

    [Fact]
    public void FutureTelemetryRejected()
    {
        var now = DateTimeOffset.UtcNow;
        var message = new TelemetryMessage(1, Guid.NewGuid(), "edge-001",
            now.AddMinutes(6), 1, Guid.NewGuid(),
            [new("temp-01", "temperature", 28m, "C", "valid")]);
        Assert.False(TelemetryValidator.TryValidate(message, now, out _));
    }

    [Fact]
    public void InvalidQualityNeverDrivesControl()
    {
        var now = DateTimeOffset.UtcNow;
        var reading = new ReadingDto(DemoIds.Barn, "edge-001", "temp-01",
            "temperature", 28m, "C", "invalid", now, now);
        Assert.False(TelemetryValidator.CanDriveControl(reading, now));
    }
}
