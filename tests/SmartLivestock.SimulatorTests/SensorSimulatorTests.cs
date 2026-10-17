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

public sealed class DemoIdsTests
{
    [Fact]
    public void SeedBarnIdsAreUniqueAndFirstKeepsLegacyId()
    {
        var ids = Enumerable.Range(1, SmartLivestock.Contracts.DemoIds.SeedBarnCount)
            .Select(SmartLivestock.Contracts.DemoIds.BarnId).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
        Assert.Equal(SmartLivestock.Contracts.DemoIds.Barn, ids[0]);
        Assert.Equal(SmartLivestock.Contracts.DemoIds.Device, SmartLivestock.Contracts.DemoIds.DeviceId(1));
    }
}

public sealed class DemoAlertsScenarioTests
{
    private static string[] Severities(int barn, string metric) =>
        Enumerable.Range(1, SensorSimulator.EscalationPeriod * 2)
            .Select(seq => SensorSimulator.Create("DEMO_ALERTS", seq, DateTimeOffset.UtcNow, barn)
                .Readings.Single(x => x.Metric == metric))
            .Select(x => SmartLivestock.Contracts.DemoAlertProfile.Severity(x.Metric, x.Value))
            .ToArray();

    [Fact]
    public void EscalatingBarnWalksNormalWarningCriticalWarningNormal()
    {
        var barn = 6; // barns are phase-shifted, so look for the full walk anywhere in two cycles
        var runs = new List<string>();
        foreach (var s in Severities(barn, SensorSimulator.EscalatingMetric(barn)))
            if (runs.Count == 0 || runs[^1] != s) runs.Add(s);
        Assert.Contains("normal,warning,critical,warning,normal", string.Join(",", runs));
    }

    [Fact]
    public void OnlyTheEscalatingMetricCrossesAndHumidityStaysValid()
    {
        var barn = 9; // 9 % 4 = 1 -> humidity escalates
        var humidityPeak = Enumerable.Range(1, SensorSimulator.EscalationPeriod)
            .Max(seq => SensorSimulator.Create("DEMO_ALERTS", seq, DateTimeOffset.UtcNow, barn)
                .Readings.Single(x => x.Metric == "humidity").Value);
        Assert.InRange(humidityPeak, 90m, 100m); // validator accepts humidity <= 100
        Assert.All(Severities(barn, "temperature"), s => Assert.Equal("normal", s));
    }

    [Fact]
    public void DemoAlertsLeavesPlainBarnsNormalAndBreaksBarnSeven()
    {
        foreach (var metric in new[] { "temperature", "humidity", "nh3", "h2s" })
            Assert.All(Severities(1, metric), s => Assert.Equal("normal", s));
        Assert.All(SensorSimulator.Create("DEMO_ALERTS", 1, DateTimeOffset.UtcNow, 7).Readings,
            x => Assert.Equal("invalid", x.Quality));
    }
}
