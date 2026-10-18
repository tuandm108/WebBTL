using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SmartLivestock.Api;
using SmartLivestock.Contracts;
using SmartLivestock.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment() &&
    (string.IsNullOrWhiteSpace(builder.Configuration["Mqtt:User"]) ||
     string.IsNullOrWhiteSpace(builder.Configuration["Mqtt:Password"])))
{
    var localCredentials = LocalDevelopmentMqttCredentials.TryLoad();
    if (localCredentials is not null)
    {
        builder.Configuration["Mqtt:User"] = localCredentials.User;
        builder.Configuration["Mqtt:Password"] = localCredentials.Password;
    }
}
builder.Host.UseSerilog((context, config) => config
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database")
        ?? throw new InvalidOperationException("ConnectionStrings:Database is required")));
builder.Services.AddSingleton<MqttConnectionStatus>();
builder.Services.AddHostedService<TelemetryMqttService>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseExceptionHandler(error => error.Run(async context =>
{
    var status = context.Features.Get<IExceptionHandlerFeature>()?.Error is BadHttpRequestException bad
        ? bad.StatusCode : StatusCodes.Status500InternalServerError;
    context.Response.StatusCode = status;
    await Results.Problem(statusCode: status, title: status == 500 ? "Internal server error" : "Bad request")
        .ExecuteAsync(context);
}));
app.UseHttpsRedirection();

app.MapGet("/health/live", () => Results.Ok(new { status = "live" }));
app.MapGet("/health/ready", async (AppDbContext db, MqttConnectionStatus mqtt, CancellationToken ct) =>
{
    if (!await db.Database.CanConnectAsync(ct))
        return Results.Problem(statusCode: 503, title: "Database unavailable");
    return mqtt.IsConnected
        ? Results.Ok(new { status = "ready" })
        : Results.Problem(statusCode: 503, title: "MQTT unavailable");
});

app.MapBarnEndpoints();
app.MapAnimalEndpoints();
app.MapAlertEndpoints();

app.MapGet("/api/barns/{barnId:guid}/environment/latest", async (Guid barnId, AppDbContext db, CancellationToken ct) =>
{
    var deviceIds = db.Devices.AsNoTracking().Where(x => x.BarnId == barnId).Select(x => x.Id);
    var readings = await db.SensorReadings.AsNoTracking()
        .Where(x => deviceIds.Contains(x.DeviceId))
        .OrderByDescending(x => x.MeasuredAt)
        .Take(500)
        .Select(x => new ReadingDto(barnId, x.Device.ExternalId, x.SensorId, x.Metric,
            x.Value, x.Unit, x.Quality, x.MeasuredAt, x.ReceivedAt))
        .ToListAsync(ct);
    return Results.Ok(readings.GroupBy(x => x.Metric).Select(x => x.First()));
});

app.MapGet("/api/barns/{barnId:guid}/environment/history", async (
    Guid barnId, string metric, DateTimeOffset from, DateTimeOffset to, int? page,
    AppDbContext db, CancellationToken ct) =>
{
    if (from >= to || to - from > TimeSpan.FromDays(7) || page is < 1 or > 1000 ||
        metric is not ("temperature" or "humidity" or "nh3" or "h2s"))
        return Results.BadRequest(new { error = "Invalid history query" });
    var deviceIds = db.Devices.AsNoTracking().Where(x => x.BarnId == barnId).Select(x => x.Id);
    var rows = await db.SensorReadings.AsNoTracking()
        .Where(x => deviceIds.Contains(x.DeviceId) && x.Metric == metric &&
                    x.MeasuredAt >= from && x.MeasuredAt < to)
        .OrderByDescending(x => x.MeasuredAt)
        .Skip(((page ?? 1) - 1) * 100).Take(100)
        .Select(x => new ReadingDto(barnId, x.Device.ExternalId, x.SensorId, x.Metric,
            x.Value, x.Unit, x.Quality, x.MeasuredAt, x.ReceivedAt))
        .ToListAsync(ct);
    return Results.Ok(rows);
});

app.MapHub<MonitoringHub>("/hubs/monitoring");
app.Run();

public partial class Program;
