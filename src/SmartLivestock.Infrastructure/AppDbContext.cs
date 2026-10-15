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
        modelBuilder.Entity<Farm>().HasData(new { Id = DemoIds.Farm, Code = "DEMO", Name = "Trang trại mẫu", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) });
        modelBuilder.Entity<Barn>().HasData(new { Id = DemoIds.Barn, FarmId = DemoIds.Farm, Code = "BARN-01", Name = "Chuồng mẫu", Capacity = 100 });
        modelBuilder.Entity<Device>().HasData(new { Id = DemoIds.Device, BarnId = DemoIds.Barn, ExternalId = "edge-001" });        modelBuilder.Entity<SensorReading>(e =>
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
