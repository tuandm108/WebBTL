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

public sealed class DemoAlertProfileTests
{
    [Theory]
    [InlineData("temperature", 29.99, "normal")]
    [InlineData("temperature", 30, "warning")]
    [InlineData("temperature", 35, "critical")]
    [InlineData("nh3", 10, "warning")]
    [InlineData("h2s", 10, "critical")]
    [InlineData("unknown", 1000, "normal")]
    public void SeverityBoundariesAreInclusive(string metric, double value, string expected) =>
        Assert.Equal(expected, SmartLivestock.Contracts.DemoAlertProfile.Severity(metric, (decimal)value));
}

public sealed class AlertHysteresisTests
{
    [Fact]
    public void WarningHoldsUntilValueDropsBelowDeadband()
    {
        // temperature warning level 30, demo deadband 1.5
        Assert.Equal("warning", DemoAlertProfile.Next("temperature", 30.2m, "normal"));
        Assert.Equal("warning", DemoAlertProfile.Next("temperature", 29.0m, "warning")); // inside the band: still open
        Assert.Equal("normal", DemoAlertProfile.Next("temperature", 28.4m, "warning"));  // below the band: clears
        Assert.Equal("normal", DemoAlertProfile.Next("temperature", 29.0m, null));       // no open alert: plain threshold
    }

    [Fact]
    public void CriticalStepsDownThroughWarningInsteadOfFlapping()
    {
        // temperature critical level 35
        Assert.Equal("critical", DemoAlertProfile.Next("temperature", 34.0m, "critical")); // inside critical band
        Assert.Equal("warning", DemoAlertProfile.Next("temperature", 32.0m, "critical"));
        Assert.Equal("warning", DemoAlertProfile.Next("temperature", 34.0m, "warning"));   // not raised without crossing 35
    }
}

public sealed class AlertDisplayScheduleTests
{
    private static readonly TimeSpan Vn = TimeSpan.FromHours(7);
    private static DateTimeOffset At(int day, int hour, int minute, int second = 0) =>
        new(2026, 10, day, hour, minute, second, Vn);

    [Fact]
    public void DailyFiresOncePerSlotAndNeverBeforeTheFirstRunIsRecorded()
    {
        var daily = new AlertDisplaySettings(AlertDisplaySchedule.Daily, "23:59");
        Assert.False(AlertDisplaySchedule.IsDue(daily, At(16, 23, 59, 30), null));            // schedule not started
        Assert.False(AlertDisplaySchedule.IsDue(daily, At(16, 23, 58), At(15, 23, 59, 30)));  // slot not reached yet
        Assert.True(AlertDisplaySchedule.IsDue(daily, At(16, 23, 59, 10), At(16, 22, 0)));    // slot reached
        Assert.False(AlertDisplaySchedule.IsDue(daily, At(17, 8, 0), At(16, 23, 59, 30)));    // already ran for that slot
    }

    [Fact]
    public void DailySlotThatPassedWhileClosedFiresOnceOnNextOpen()
    {
        var daily = new AlertDisplaySettings(AlertDisplaySchedule.Daily, "23:59");
        Assert.True(AlertDisplaySchedule.IsDue(daily, At(17, 9, 0), At(16, 20, 0)));
    }

    [Fact]
    public void IntervalFiresExactlyWhenTheMinutesHavePassed()
    {
        var every30 = new AlertDisplaySettings(AlertDisplaySchedule.Interval, Minutes: 30);
        Assert.False(AlertDisplaySchedule.IsDue(every30, At(16, 10, 29), At(16, 10, 0)));
        Assert.True(AlertDisplaySchedule.IsDue(every30, At(16, 10, 30), At(16, 10, 0)));
    }

    [Theory]
    [InlineData(AlertDisplaySchedule.Immediate)]
    [InlineData(AlertDisplaySchedule.Off)]
    public void ImmediateAndOffNeverSchedule(string mode)
    {
        var settings = new AlertDisplaySettings(mode);
        Assert.False(AlertDisplaySchedule.IsDue(settings, At(17, 12, 0), At(10, 0, 0)));
        Assert.Null(AlertDisplaySchedule.NextDue(settings, At(17, 12, 0), At(10, 0, 0)));
    }

    [Fact]
    public void NextDueIsTomorrowsSlotAfterTodaysRunAndNowWhenOverdue()
    {
        var daily = new AlertDisplaySettings(AlertDisplaySchedule.Daily, "23:59");
        Assert.Equal(At(17, 23, 59), AlertDisplaySchedule.NextDue(daily, At(16, 23, 59, 40), At(16, 23, 59, 30)));
        Assert.Equal(At(17, 9, 0), AlertDisplaySchedule.NextDue(daily, At(17, 9, 0), At(16, 20, 0)));
        var every30 = new AlertDisplaySettings(AlertDisplaySchedule.Interval, Minutes: 30);
        Assert.Equal(At(16, 10, 30), AlertDisplaySchedule.NextDue(every30, At(16, 10, 5), At(16, 10, 0)));
    }

    [Fact]
    public void NormalizeRepairsBadStoredValues()
    {
        var fixedUp = AlertDisplaySchedule.Normalize(new AlertDisplaySettings("weird", "25:99", 99999, true));
        Assert.Equal(new AlertDisplaySettings(AlertDisplaySchedule.Daily, "23:59", 1440, true), fixedUp);
        Assert.Equal(new AlertDisplaySettings(), AlertDisplaySchedule.Normalize(null));
    }
}
