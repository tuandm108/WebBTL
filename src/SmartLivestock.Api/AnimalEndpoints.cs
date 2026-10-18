using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using SmartLivestock.Contracts;
using SmartLivestock.Domain;
using SmartLivestock.Infrastructure;

namespace SmartLivestock.Api;

public static class AnimalEndpoints
{
    private static readonly Guid FarmId = DemoIds.Farm;

    public static void MapAnimalEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/species", () => Results.Ok(LivestockSpecies.All));

        app.MapGet("/api/animals", async (Guid? barnId, AppDbContext db, CancellationToken ct) =>
        {
            var query = db.Animals.AsNoTracking().Where(x => x.FarmId == FarmId);
            if (barnId.HasValue) query = query.Where(x => x.BarnId == barnId.Value);
            return Results.Ok(await query.OrderBy(x => x.Tag).Select(ToDto).ToListAsync(ct));
        });

        app.MapGet("/api/animals/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
            await db.Animals.AsNoTracking().Where(x => x.Id == id && x.FarmId == FarmId)
                .Select(ToDto).FirstOrDefaultAsync(ct) is { } animal
                ? Results.Ok(animal) : Results.NotFound());

        app.MapPost("/api/animals", async (AnimalInput input, AppDbContext db, IHubContext<MonitoringHub> hub, CancellationToken ct) =>
        {
            if (Validate(input) is { } invalid) return invalid;
            var species = LivestockSpecies.Normalize(input.Species);
            if (species is null)
                return Results.BadRequest(new { error = "Select a species from the catalog" });
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var barn = await db.Barns.FirstOrDefaultAsync(x => x.Id == input.BarnId && x.FarmId == FarmId, ct);
            if (barn is null) return Results.BadRequest(new { error = "Barn does not exist" });
            if (await db.Animals.CountAsync(x => x.BarnId == barn.Id, ct) >= barn.Capacity)
                return Results.Conflict(new { error = "Barn is at capacity" });

            var animal = new Animal
            {
                Id = Guid.NewGuid(), FarmId = FarmId, BarnId = barn.Id,
                Tag = input.Tag.Trim(), Name = input.Name?.Trim() ?? "", Species = species,
                RegisteredAt = DateTimeOffset.UtcNow
            };
            db.Animals.Add(animal);
            if (await SaveAsync(db, ct) is { } conflict) return conflict;
            await transaction.CommitAsync(ct);
            await hub.Clients.Group(barn.Id.ToString("D")).SendAsync("animalChanged", barn.Id, ct);
            return Results.Created($"/api/animals/{animal.Id}",
                new AnimalDto(animal.Id, animal.Tag, animal.Name, animal.Species, animal.BarnId,
                    barn.Code, animal.RegisteredAt, animal.IsDemo));
        });

        app.MapPut("/api/animals/{id:guid}", async (Guid id, AnimalInput input, AppDbContext db, IHubContext<MonitoringHub> hub, CancellationToken ct) =>
        {
            if (Validate(input) is { } invalid) return invalid;
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var animal = await db.Animals.FirstOrDefaultAsync(x => x.Id == id && x.FarmId == FarmId, ct);
            if (animal is null) return Results.NotFound();
            var species = LivestockSpecies.Normalize(input.Species);
            if (species is null && !string.Equals(animal.Species, input.Species.Trim(), StringComparison.OrdinalIgnoreCase))
                return Results.BadRequest(new { error = "Select a species from the catalog" });
            var barn = await db.Barns.FirstOrDefaultAsync(x => x.Id == input.BarnId && x.FarmId == FarmId, ct);
            if (barn is null) return Results.BadRequest(new { error = "Barn does not exist" });
            if (animal.BarnId != barn.Id &&
                await db.Animals.CountAsync(x => x.BarnId == barn.Id, ct) >= barn.Capacity)
                return Results.Conflict(new { error = "Barn is at capacity" });

            var previousBarnId = animal.BarnId;
            animal.Tag = input.Tag.Trim();
            animal.Name = input.Name?.Trim() ?? "";
            animal.Species = species ?? animal.Species;
            animal.BarnId = barn.Id;
            if (await SaveAsync(db, ct) is { } conflict) return conflict;
            await transaction.CommitAsync(ct);
            await hub.Clients.Group(barn.Id.ToString("D")).SendAsync("animalChanged", barn.Id, ct);
            if (previousBarnId != barn.Id)
                await hub.Clients.Group(previousBarnId.ToString("D")).SendAsync("animalChanged", previousBarnId, ct);
            return Results.NoContent();
        });

        app.MapDelete("/api/animals/{id:guid}", async (Guid id, AppDbContext db, IHubContext<MonitoringHub> hub, CancellationToken ct) =>
        {
            var animal = await db.Animals.FirstOrDefaultAsync(x => x.Id == id && x.FarmId == FarmId, ct);
            if (animal is null) return Results.NotFound();
            db.Animals.Remove(animal);
            await db.SaveChangesAsync(ct);
            await hub.Clients.Group(animal.BarnId.ToString("D")).SendAsync("animalChanged", animal.BarnId, ct);
            return Results.NoContent();
        });
    }

    private static readonly System.Linq.Expressions.Expression<Func<Animal, AnimalDto>> ToDto =
        x => new AnimalDto(x.Id, x.Tag, x.Name, x.Species, x.BarnId, x.Barn.Code, x.RegisteredAt, x.IsDemo);

    private static IResult? Validate(AnimalInput? input) =>
        input is null || input.BarnId == Guid.Empty ||
        string.IsNullOrWhiteSpace(input.Tag) || input.Tag.Trim().Length > 64 ||
        input.Name?.Trim().Length > 200 ||
        string.IsNullOrWhiteSpace(input.Species) || input.Species.Trim().Length > 100
            ? Results.BadRequest(new { error = "Tag, species and barn are required; name is limited to 200 characters" })
            : null;

    private static async Task<IResult?> SaveAsync(AppDbContext db, CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); return null; }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return Results.Conflict(new { error = "Animal tag already exists" });
        }
    }
}
