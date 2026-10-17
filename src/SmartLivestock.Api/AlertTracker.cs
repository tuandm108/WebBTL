using Microsoft.EntityFrameworkCore;
using SmartLivestock.Contracts;
using SmartLivestock.Domain;
using SmartLivestock.Infrastructure;

namespace SmartLivestock.Api;

/// <summary>
/// Turns each accepted telemetry message into alert episodes. Changes are tracked on the context, so the
/// caller saves them in the same transaction as the readings and broadcasts only after that succeeds.
/// </summary>
public static class AlertTracker
{
    public static async Task<List<(Alert Alert, string Change)>> ApplyAsync(
        AppDbContext db, Device device, TelemetryMessage message, CancellationToken ct)
    {
        var changes = new List<(Alert, string)>();
        var at = message.TimestampUtc;
        var open = await db.Alerts.Where(a => a.BarnId == device.BarnId && a.State == AlertState.Active)
            .ToListAsync(ct);

        // Thresholds only from trustworthy data; the highest value per metric is the conservative one.
        foreach (var group in message.Readings.Where(x => x.Quality == "valid").GroupBy(x => x.Metric))
        {
            var value = group.Max(x => x.Value);
            var alert = open.FirstOrDefault(a => a.DedupeKey == group.Key);
            var next = DemoAlertProfile.Next(group.Key, value, alert?.Severity);
            if (alert is null)
            {
                if (next == DemoAlertProfile.Normal) continue;
                var raised = New(device, AlertType.Threshold, group.Key, next, at);
                raised.PeakValue = value;
                raised.Unit = group.First().Unit;
                db.Alerts.Add(raised);
                changes.Add((raised, "raised"));
            }
            else if (next == DemoAlertProfile.Normal)
            {
                Close(alert, at);
                changes.Add((alert, "cleared"));
            }
            else
            {
                Touch(alert, at);
                if (value > alert.PeakValue) alert.PeakValue = value;
                if (AlertSeverity.Rank(next) > AlertSeverity.Rank(alert.Severity))
                {
                    alert.Severity = next;
                    alert.ReadAt = null; // new information: needs a fresh look
                    changes.Add((alert, "escalated"));
                }
            }
        }

        // Any reading that is not valid (invalid or stale) is a sensor fault for the barn, one alert at a time.
        var faulty = message.Readings.Any(x => x.Quality != "valid");
        var fault = open.FirstOrDefault(a => a.DedupeKey == "fault");
        if (faulty && fault is null)
        {
            var raised = New(device, AlertType.SensorFault, null, AlertSeverity.Fault, at);
            db.Alerts.Add(raised);
            changes.Add((raised, "raised"));
        }
        else if (faulty) Touch(fault!, at);
        else if (fault is not null)
        {
            Close(fault, at);
            changes.Add((fault, "cleared"));
        }
        return changes;
    }

    public static AlertDto ToDto(Alert a, Barn barn) => new(a.Id, a.BarnId, barn.Code, barn.Name, a.Type,
        a.Metric, a.Severity, a.State, a.FirstSeenAt, a.LastSeenAt, a.ClearedAt, a.PeakValue, a.Unit, a.ReadAt);

    private static Alert New(Device device, string type, string? metric, string severity, DateTimeOffset at) => new()
    {
        Id = Guid.NewGuid(), BarnId = device.BarnId, Type = type, Metric = metric, Severity = severity,
        State = AlertState.Active, DedupeKey = metric ?? "fault", FirstSeenAt = at, LastSeenAt = at
    };

    private static void Touch(Alert alert, DateTimeOffset at) =>
        alert.LastSeenAt = at > alert.LastSeenAt ? at : alert.LastSeenAt;

    private static void Close(Alert alert, DateTimeOffset at)
    {
        alert.State = AlertState.Cleared;
        alert.ClearedAt = at;
        Touch(alert, at);
    }
}
