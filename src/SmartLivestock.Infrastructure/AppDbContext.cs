using Microsoft.EntityFrameworkCore;
using SmartLivestock.Domain;
using SmartLivestock.Contracts;

namespace SmartLivestock.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Farm> Farms => Set<Farm>();
    public DbSet<Barn> Barns => Set<Barn>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<Animal> Animals => Set<Animal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Farm>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Code).HasMaxLength(64);
            e.Property(x => x.Name).HasMaxLength(200);
        });
        modelBuilder.Entity<Barn>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.FarmId, x.Code }).IsUnique();
            e.Property(x => x.Code).HasMaxLength(64);
            e.Property(x => x.Name).HasMaxLength(200);
        });
        modelBuilder.Entity<Device>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ExternalId).IsUnique();
            e.Property(x => x.ExternalId).HasMaxLength(128);
        });
        modelBuilder.Entity<Animal>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.FarmId, x.Tag }).IsUnique();
            e.HasIndex(x => x.BarnId);
            e.Property(x => x.Tag).HasMaxLength(64);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Species).HasMaxLength(100);
            e.Property(x => x.IsDemo).HasDefaultValue(false);
            e.HasOne(x => x.Barn).WithMany(x => x.Animals).HasForeignKey(x => x.BarnId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Farm>().HasData(new { Id = DemoIds.Farm, Code = "DEMO", Name = "Trang trại mẫu", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) });
        modelBuilder.Entity<Barn>().HasData(new { Id = DemoIds.Barn, FarmId = DemoIds.Farm, Code = "BARN-01", Name = "Chuồng mẫu", Capacity = 100, Monitored = true });
        modelBuilder.Entity<Device>().HasData(new { Id = DemoIds.Device, BarnId = DemoIds.Barn, ExternalId = "edge-001" });
        var extra = Enumerable.Range(2, DemoIds.SeedBarnCount - 1).ToList();
        modelBuilder.Entity<Barn>().HasData(extra.Select(n => new
        {
            Id = DemoIds.BarnId(n), FarmId = DemoIds.Farm, Code = $"BARN-{n:00}",
            Name = $"Chuồng {n:00}", Capacity = 60 + n % 5 * 20, Monitored = true
        }));
        modelBuilder.Entity<Device>().HasData(extra.Select(n => new
        {
            Id = DemoIds.DeviceId(n), BarnId = DemoIds.BarnId(n), ExternalId = DemoIds.DeviceExternalId(n)
        }));
        modelBuilder.Entity<Alert>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).HasMaxLength(32);
            e.Property(x => x.Metric).HasMaxLength(32);
            e.Property(x => x.Severity).HasMaxLength(16);
            e.Property(x => x.State).HasMaxLength(16);
            e.Property(x => x.DedupeKey).HasMaxLength(64);
            e.Property(x => x.Unit).HasMaxLength(16);
            e.Property(x => x.PeakValue).HasPrecision(14, 4);
            // At most one open alert per barn and condition; history rows are unconstrained.
            e.HasIndex(x => new { x.BarnId, x.DedupeKey }).IsUnique().HasFilter("[State] = 'active'");
            e.HasIndex(x => x.FirstSeenAt);
            e.HasOne(x => x.Barn).WithMany().HasForeignKey(x => x.BarnId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<SensorReading>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.DeviceId, x.MessageId, x.SensorId }).IsUnique();
            e.HasIndex(x => new { x.DeviceId, x.Metric, x.MeasuredAt });
            e.Property(x => x.SensorId).HasMaxLength(128);
            e.Property(x => x.Metric).HasMaxLength(32);
            e.Property(x => x.Unit).HasMaxLength(16);
            e.Property(x => x.Quality).HasMaxLength(16);
            e.Property(x => x.Value).HasPrecision(14, 4);
        });
    }
}
