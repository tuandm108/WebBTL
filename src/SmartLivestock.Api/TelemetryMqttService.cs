using System.Buffers;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using MQTTnet;
using MQTTnet.Protocol;
using SmartLivestock.Application;
using SmartLivestock.Contracts;
using SmartLivestock.Domain;
using SmartLivestock.Infrastructure;

namespace SmartLivestock.Api;

public sealed class TelemetryMqttService(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    IHubContext<MonitoringHub> hub,
    ILogger<TelemetryMqttService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var host = configuration["Mqtt:Host"] ?? "localhost";
        var port = configuration.GetValue("Mqtt:Port", 1883);
        var user = configuration["Mqtt:User"];
        var password = configuration["Mqtt:Password"];
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("MQTT credentials absent; telemetry ingestion disabled");
            return;
        }

        var factory = new MqttClientFactory();
        using var client = factory.CreateMqttClient();
        client.ApplicationMessageReceivedAsync += async args =>
        {
            args.AutoAcknowledge = false;
            try
            {
                await ProcessMessageAsync(args.ApplicationMessage.Topic,
                    args.ApplicationMessage.Payload.ToArray(), stoppingToken);
                await args.AcknowledgeAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Telemetry processing failed; reconnect will retry pending QoS 1 message");
            }
        };
        var options = new MqttClientOptionsBuilder().WithTcpServer(host, port)
            .WithCredentials(user, password).WithClientId("smartlivestock-api").Build();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!client.IsConnected)
                {
                    await client.ConnectAsync(options, stoppingToken);
                    var subscription = factory.CreateSubscribeOptionsBuilder()
                        .WithTopicFilter(f => f.WithTopic("smartlivestock/v1/+/+/+/telemetry")
                            .WithAtLeastOnceQoS()).Build();
                    await client.SubscribeAsync(subscription, stoppingToken);
                    logger.LogInformation("MQTT telemetry subscription active");
                }
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "MQTT disconnected; retrying");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ProcessMessageAsync(string topic, byte[] payload, CancellationToken ct)
    {
        var segments = topic.Split('/');
        if (segments.Length != 6 || segments[0] != "smartlivestock" ||
            segments[1] != "v1" || segments[5] != "telemetry" ||
            !Guid.TryParse(segments[2], out var farmId) ||
            !Guid.TryParse(segments[3], out var barnId))
        {
            logger.LogWarning("Invalid telemetry topic");
            return;
        }

        TelemetryMessage? message;
        try { message = JsonSerializer.Deserialize<TelemetryMessage>(payload,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)); }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Invalid telemetry JSON");
            return;
        }
        var receivedAt = DateTimeOffset.UtcNow;
        if (!TelemetryValidator.TryValidate(message, receivedAt, out var error) ||
            message!.DeviceId != segments[4])
        {
            logger.LogWarning("Rejected telemetry: {Error}", error);
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var device = await db.Devices.Include(x => x.Barn).FirstOrDefaultAsync(x =>
            x.ExternalId == message.DeviceId && x.BarnId == barnId &&
            x.Barn.FarmId == farmId, ct);
        if (device is null)
        {
            logger.LogWarning("Telemetry from unregistered device {DeviceId}", message.DeviceId);
            return;
        }
        if (await db.SensorReadings.AnyAsync(x => x.DeviceId == device.Id &&
            x.MessageId == message.MessageId, ct)) return;

        var rows = message.Readings.Select(reading => new SensorReading
        {
            DeviceId = device.Id,
            MessageId = message.MessageId,
            BootId = message.BootId,
            Sequence = message.Sequence,
            SensorId = reading.SensorId,
            Metric = reading.Metric,
            Value = reading.Value,
            Unit = reading.Unit,
            Quality = reading.Quality,
            MeasuredAt = message.TimestampUtc,
            ReceivedAt = receivedAt
        }).ToList();
        db.SensorReadings.AddRange(rows);
        device.LastSeenAt = receivedAt;
        var alertChanges = await AlertTracker.ApplyAsync(db, device, message, ct);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException
            { Number: 2601 or 2627 })
        {
            return;
        }
        foreach (var row in rows)
        {
            var dto = new ReadingDto(barnId, device.ExternalId, row.SensorId, row.Metric,
                row.Value, row.Unit, row.Quality, row.MeasuredAt, row.ReceivedAt);
            await hub.Clients.Group(barnId.ToString("D")).SendAsync("telemetryUpdated", dto, ct);
        }
        foreach (var (alert, change) in alertChanges)
            await hub.Clients.Group(MonitoringHub.AlertsGroup)
                .SendAsync("alertChanged", new AlertChangedDto(change, AlertTracker.ToDto(alert, device.Barn)), ct);
    }
}
