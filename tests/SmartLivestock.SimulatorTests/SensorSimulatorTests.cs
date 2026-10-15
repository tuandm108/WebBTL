using SmartLivestock.IoTSimulator;

namespace SmartLivestock.SimulatorTests;

public sealed class SensorSimulatorTests
{
    [Fact]
    public void FailureScenarioMarksReadingsInvalid()
    {
        var sample = SensorSimulator.Create("SENSOR_FAILURE", 1, DateTimeOffset.UtcNow);
        Assert.All(sample.Readings, x => Assert.Equal("invalid", x.Quality));
    }

    [Fact]
    public void HighTemperatureChangesOnlyTemperature()
    {
        var now = DateTimeOffset.UtcNow;
        var normal = SensorSimulator.Create("NORMAL", 1, now);
        var hot = SensorSimulator.Create("HIGH_TEMPERATURE", 1, now);
        Assert.Equal(normal.Readings[0].Value + 8m, hot.Readings[0].Value);
        Assert.Equal(normal.Readings[1].Value, hot.Readings[1].Value);
    }
}
