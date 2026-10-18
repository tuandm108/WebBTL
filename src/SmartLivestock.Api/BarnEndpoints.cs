using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartLivestock.Contracts;
using SmartLivestock.Domain;
using SmartLivestock.Infrastructure;

namespace SmartLivestock.Api;

public static class BarnEndpoints
{
    // ponytail: single demo farm until farm management (F10) exists
    private static readonly Guid FarmId = DemoIds.Farm;
    private static readonly string[] Metrics = ["temperature", "humidity", "nh3", "h2s"];

    public static void MapBarnEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/barns", async (AppDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Barns.AsNoTracking().Where(x => x.FarmId == FarmId)
                .OrderBy(x => x.Code).Select(ToDto).ToListAsync(ct)));

        app.MapGet("/api/monitoring/barns", async (int? page, int? pageSize, AppDbContext db, CancellationToken ct) =>
        {
            var size = Math.Clamp(pageSize ?? 5, 1, 50);
            var query = db.Barns.AsNoTracking().Where(x => x.FarmId == FarmId && x.Monitored);
            var total = await query.CountAsync(ct);
            var current = Math.Clamp(page ?? 1, 1, Math.Max(1, (total + size - 1) / size));
            var barns = await query.OrderBy(x => x.Code).Skip((current - 1) * size).Take(size)
                .Select(ToDto).ToListAsync(ct);
            var ids = barns.Select(x => x.Id).ToList();

            // One indexed TOP 1 per device and metric, instead of scanning the readings table.
            var latest = new List<ReadingDto>();
            foreach (var metric in Metrics)
            {
                var rows = await db.Devices.AsNoTracking().Where(d => ids.Contains(d.BarnId))
                    .Select(d => d.Readings.Where(r => r.Metric == metric)
                        .OrderByDescending(r => r.MeasuredAt)
                        .Select(r => new ReadingDto(d.BarnId, d.ExternalId, r.SensorId, r.Metric,
                            r.Value, r.Unit, r.Quality, r.MeasuredAt, r.ReceivedAt))
                        .FirstOrDefault())
                    .ToListAsync(ct);
                latest.AddRange(rows.OfType<ReadingDto>());
            }
            var animalGroups = await db.Animals.AsNoTracking().Where(x => ids.Contains(x.BarnId))
                .GroupBy(x => new { x.BarnId, x.Species })
                .Select(x => new { x.Key.BarnId, x.Key.Species, Count = x.Count() })
                .ToListAsync(ct);
            var items = barns.Select(b => new MonitoredBarnDto(b,
                latest.Where(r => r.BarnId == b.Id).ToList(),
                animalGroups.Where(x => x.BarnId == b.Id)
                    .GroupBy(x => LivestockSpecies.Normalize(x.Species) ?? x.Species)
                    .Select(x => new SpeciesCountDto(x.Key, LivestockSpecies.NameOf(x.Key), x.Sum(y => y.Count)))
                    .OrderByDescending(x => x.Count).ThenBy(x => x.Name)
                    .ToList())).ToList();
            return Results.Ok(new PagedResult<MonitoredBarnDto>(current, size, total, items));
        });

        app.MapGet("/api/barns/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
            await db.Barns.AsNoTracking().Where(x => x.Id == id && x.FarmId == FarmId)
                .Select(ToDto).FirstOrDefaultAsync(ct) is { } barn
                ? Results.Ok(barn) : Results.NotFound());

        app.MapPost("/api/barns", async (BarnInput input, AppDbContext db, CancellationToken ct) =>
        {
            if (Validate(input) is { } invalid) return invalid;
            var barn = new Barn { Id = Guid.NewGuid(), FarmId = FarmId, Code = input.Code.Trim(),
                Name = input.Name.Trim(), Capacity = input.Capacity, Monitored = input.Monitored };
            db.Barns.Add(barn);
            if (await SaveAsync(db, ct) is { } conflict) return conflict;
            return Results.Created($"/api/barns/{barn.Id}", new BarnDto(barn.Id, barn.Code, barn.Name, barn.Capacity, 0, barn.Monitored, 0));
        });

        app.MapPut("/api/barns/monitored", async (MonitoredInput input, AppDbContext db, CancellationToken ct) =>
        {
            if (input?.Ids is null || input.Ids.Count is < 1 or > 500)
                return Results.BadRequest(new { error = "Provide 1-500 barn IDs" });
            var ids = input.Ids.Distinct().ToList();
            var updated = await db.Barns.Where(x => x.FarmId == FarmId && ids.Contains(x.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Monitored, input.Monitored), ct);
            return Results.Ok(new { updated });
        });

        app.MapPut("/api/barns/{id:guid}", async (Guid id, BarnInput input, AppDbContext db, CancellationToken ct) =>
        {
            if (Validate(input) is { } invalid) return invalid;
            var barn = await db.Barns.FirstOrDefaultAsync(x => x.Id == id && x.FarmId == FarmId, ct);
            if (barn is null) return Results.NotFound();
            if (await db.Animals.CountAsync(x => x.BarnId == id, ct) > input.Capacity)
                return Results.Conflict(new { error = "Barn capacity is below the current animal count" });
            barn.Code = input.Code.Trim();
            barn.Name = input.Name.Trim();
            barn.Capacity = input.Capacity;
            barn.Monitored = input.Monitored;
            return await SaveAsync(db, ct) ?? Results.NoContent();
        });

        app.MapDelete("/api/barns/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var barn = await db.Barns.FirstOrDefaultAsync(x => x.Id == id && x.FarmId == FarmId, ct);
            if (barn is null) return Results.NotFound();
            // Devices cascade to sensor readings, so refuse instead of silently wiping history.
            if (await db.Devices.AnyAsync(x => x.BarnId == id, ct))
                return Results.Conflict(new { error = "Barn still has devices; remove them first" });
            if (await db.Animals.AnyAsync(x => x.BarnId == id, ct))
                return Results.Conflict(new { error = "Barn still has animals; move or remove them first" });
            db.Barns.Remove(barn);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }

    private static readonly System.Linq.Expressions.Expression<Func<Barn, BarnDto>> ToDto =
        x => new BarnDto(x.Id, x.Code, x.Name, x.Capacity, x.Devices.Count, x.Monitored, x.Animals.Count);

    private static IResult? Validate(BarnInput? input) =>
        input is null || string.IsNullOrWhiteSpace(input.Code) || input.Code.Trim().Length > 64 ||
        string.IsNullOrWhiteSpace(input.Name) || input.Name.Trim().Length > 200 ||
        input.Capacity is < 1 or > 100_000
            ? Results.BadRequest(new { error = "Invalid barn: code 1-64 chars, name 1-200 chars, capacity 1-100000" })
            : null;

    private static async Task<IResult?> SaveAsync(AppDbContext db, CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); return null; }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return Results.Conflict(new { error = "Barn code already exists" });
        }
    }
}
