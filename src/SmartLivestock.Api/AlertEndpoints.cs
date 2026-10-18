using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartLivestock.Contracts;
using SmartLivestock.Domain;
using SmartLivestock.Infrastructure;

namespace SmartLivestock.Api;

public static class AlertEndpoints
{
    private static readonly string[] Severities = [AlertSeverity.Warning, AlertSeverity.Critical, AlertSeverity.Fault];
    private static readonly string[] States = [AlertState.Active, AlertState.Cleared];

    public static void MapAlertEndpoints(this IEndpointRouteBuilder app)
    {
        // History: alerts whose lifetime overlaps [from, to]; either bound may be omitted.
        // unread=true only unread, unread=false only read, omitted both.
        app.MapGet("/api/alerts", async (DateTimeOffset? from, DateTimeOffset? to, Guid? barnId, string? severity,
            string? state, bool? unread, int? page, int? pageSize, AppDbContext db, CancellationToken ct) =>
        {
            if (!ValidFilters(from, to, severity, state) || page is < 1 or > 10_000)
                return Results.BadRequest(new { error = "Invalid alert query" });

            var query = Filtered(db.Alerts.AsNoTracking(), from, to, barnId, severity, state);
            if (unread is { } u) query = u ? query.Where(a => a.ReadAt == null) : query.Where(a => a.ReadAt != null);

            var size = Math.Clamp(pageSize ?? 20, 1, 100);
            var total = await query.CountAsync(ct);
            var current = Math.Clamp(page ?? 1, 1, Math.Max(1, (total + size - 1) / size));
            var items = await query.OrderByDescending(a => a.FirstSeenAt).ThenBy(a => a.Id)
                .Skip((current - 1) * size).Take(size)
                .Select(a => new AlertDto(a.Id, a.BarnId, a.Barn.Code, a.Barn.Name, a.Type, a.Metric, a.Severity,
                    a.State, a.FirstSeenAt, a.LastSeenAt, a.ClearedAt, a.PeakValue, a.Unit, a.ReadAt))
                .ToListAsync(ct);
            var barnIds = items.Select(a => a.BarnId).Distinct().ToList();
            var animalGroups = await db.Animals.AsNoTracking().Where(a => barnIds.Contains(a.BarnId))
                .GroupBy(a => new { a.BarnId, a.Species })
                .Select(g => new { g.Key.BarnId, g.Key.Species, Count = g.Count() })
                .ToListAsync(ct);
            var currentAnimals = animalGroups.GroupBy(g => g.BarnId).ToDictionary(g => g.Key,
                g => g.GroupBy(x => LivestockSpecies.Normalize(x.Species) ?? x.Species)
                    .Select(x => new SpeciesCountDto(x.Key, LivestockSpecies.NameOf(x.Key), x.Sum(y => y.Count)))
                    .OrderByDescending(x => x.Count).ThenBy(x => x.Name).ToList());
            var history = items.Select(a =>
            {
                var species = currentAnimals.GetValueOrDefault(a.BarnId) ?? [];
                return a with { CurrentAnimalCount = species.Sum(x => x.Count), CurrentSpecies = species };
            }).ToList();
            return Results.Ok(new PagedResult<AlertDto>(current, size, total, history));
        });

        // Open alerts by severity, plus how many alerts (any state) are still unread.
        app.MapGet("/api/alerts/active-count", async (AppDbContext db, CancellationToken ct) =>
        {
            var rows = await db.Alerts.AsNoTracking().Where(a => a.State == AlertState.Active)
                .GroupBy(a => a.Severity).Select(g => new { Severity = g.Key, Count = g.Count() }).ToListAsync(ct);
            int Of(string severity) => rows.Where(r => r.Severity == severity).Sum(r => r.Count);
            var unread = await db.Alerts.CountAsync(a => a.ReadAt == null, ct);
            return Results.Ok(new AlertCountsDto(rows.Sum(r => r.Count), Of(AlertSeverity.Critical),
                Of(AlertSeverity.Warning), Of(AlertSeverity.Fault), unread));
        });

        // Digest for the alert dialog: one entry per barn, worst first. unreadOnly skips what the user already read.
        app.MapGet("/api/alerts/summary", async (DateTimeOffset from, DateTimeOffset to, bool? unreadOnly,
            AppDbContext db, CancellationToken ct) =>
        {
            if (from >= to || to - from > TimeSpan.FromDays(31))
                return Results.BadRequest(new { error = "Invalid summary window" });

            var query = Filtered(db.Alerts.AsNoTracking(), from, to, null, null, null);
            if (unreadOnly == true) query = query.Where(a => a.ReadAt == null);
            var alerts = await query.OrderByDescending(a => a.FirstSeenAt).Take(1000)
                .Select(a => new AlertDto(a.Id, a.BarnId, a.Barn.Code, a.Barn.Name, a.Type, a.Metric, a.Severity,
                    a.State, a.FirstSeenAt, a.LastSeenAt, a.ClearedAt, a.PeakValue, a.Unit, a.ReadAt))
                .ToListAsync(ct);

            var barns = alerts.GroupBy(a => a.BarnId).Select(g => new AlertSummaryBarnDto(
                    g.Key, g.First().BarnCode, g.First().BarnName,
                    g.OrderByDescending(a => AlertSeverity.Rank(a.Severity)).First().Severity, g.Count(),
                    g.Min(a => a.FirstSeenAt), g.Max(a => a.LastSeenAt),
                    g.OrderByDescending(a => AlertSeverity.Rank(a.Severity)).ThenByDescending(a => a.FirstSeenAt)
                        .Take(5).ToList()))
                .OrderByDescending(b => AlertSeverity.Rank(b.WorstSeverity)).ThenBy(b => b.BarnCode).ToList();

            return Results.Ok(new AlertSummaryDto(from, to, alerts.Count,
                alerts.Count(a => a.Severity == AlertSeverity.Critical),
                alerts.Count(a => a.Severity == AlertSeverity.Warning),
                alerts.Count(a => a.Severity == AlertSeverity.Fault), barns));
        });

        // Read state is shared (there are no user accounts yet); F08 should record who read it.
        app.MapPost("/api/alerts/mark-read", async (AlertReadRequest request, AppDbContext db,
            IHubContext<MonitoringHub> hub, CancellationToken ct) =>
        {
            var hasIds = request.Ids is { Count: > 0 and <= 500 };
            if (hasIds == request.All || !ValidFilters(request.From, request.To, request.Severity, request.State))
                return Results.BadRequest(new { error = "Provide 1-500 ids, or all=true with valid filters" });

            var query = hasIds
                ? db.Alerts.Where(a => request.Ids!.Contains(a.Id))
                : Filtered(db.Alerts, request.From, request.To, request.BarnId, request.Severity, request.State);
            DateTimeOffset? stamp = request.Read ? DateTimeOffset.UtcNow : null;
            var updated = await query.ExecuteUpdateAsync(s => s.SetProperty(a => a.ReadAt, stamp), ct);
            if (updated > 0) await hub.Clients.Group(MonitoringHub.AlertsGroup).SendAsync("alertsRead", updated, ct);
            return Results.Ok(new { updated });
        });
    }

    private static bool ValidFilters(DateTimeOffset? from, DateTimeOffset? to, string? severity, string? state) =>
        (severity is null || Severities.Contains(severity)) && (state is null || States.Contains(state)) &&
        !(from is not null && to is not null && from >= to);

    private static IQueryable<Alert> Filtered(IQueryable<Alert> query, DateTimeOffset? from, DateTimeOffset? to,
        Guid? barnId, string? severity, string? state)
    {
        // Overlap with the window: started before "to" and still open or last seen after "from".
        if (to is not null) query = query.Where(a => a.FirstSeenAt <= to);
        if (from is not null) query = query.Where(a => a.State == AlertState.Active || a.LastSeenAt >= from);
        if (barnId is not null) query = query.Where(a => a.BarnId == barnId);
        if (severity is not null) query = query.Where(a => a.Severity == severity);
        if (state is not null) query = query.Where(a => a.State == state);
        return query;
    }
}
