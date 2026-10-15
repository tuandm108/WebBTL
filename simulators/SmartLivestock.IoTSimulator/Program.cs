using System.Text.Json;
using MQTTnet;
using MQTTnet.Protocol;
using SmartLivestock.Contracts;
using SmartLivestock.IoTSimulator;

var user = Environment.GetEnvironmentVariable("MQTT_USER");
var password = Environment.GetEnvironmentVariable("MQTT_PASSWORD");
LocalDevelopmentMqttCredentials? localCredentials = null;
if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
{
    localCredentials = LocalDevelopmentMqttCredentials.TryLoad();
    user = localCredentials?.User;
    password = localCredentials?.Password;
}
if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
    throw new InvalidOperationException("Run scripts/setup-local-mqtt.ps1 or set MQTT_USER and MQTT_PASSWORD");
var host = Environment.GetEnvironmentVariable("MQTT_HOST") ?? "localhost";
var port = int.TryParse(Environment.GetEnvironmentVariable("MQTT_PORT"), out var configuredPort)
    ? configuredPort : localCredentials is null ? 1883 : 1884;
var scenario = Environment.GetEnvironmentVariable("SCENARIO") ?? "NORMAL";
var topic = $"smartlivestock/v1/{DemoIds.Farm:D}/{DemoIds.Barn:D}/edge-001/telemetry";
var factory = new MqttClientFactory();
using var client = factory.CreateMqttClient();
var options = new MqttClientOptionsBuilder().WithTcpServer(host, port)
    .WithCredentials(user, password).WithClientId($"simulator-{Guid.NewGuid():N}").Build();
using var shutdown = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; shutdown.Cancel(); };
long sequence = 0;
Console.WriteLine($"Simulator scenario: {scenario}");
while (!shutdown.IsCancellationRequested)
{
    try
    {
        if (!client.IsConnected) await client.ConnectAsync(options, shutdown.Token);
        var message = SensorSimulator.Create(scenario, ++sequence, DateTimeOffset.UtcNow);
        var payload = JsonSerializer.Serialize(message, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var mqttMessage = new MqttApplicationMessageBuilder().WithTopic(topic)
            .WithPayload(payload).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();
        await client.PublishAsync(mqttMessage, shutdown.Token);
        Console.WriteLine($"{message.TimestampUtc:O} seq={sequence} temperature={message.Readings[0].Value}");
        await Task.Delay(TimeSpan.FromSeconds(5), shutdown.Token);
    }
    catch (OperationCanceledException) when (shutdown.IsCancellationRequested) { break; }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"MQTT: {ex.Message}; retrying");
        await Task.Delay(TimeSpan.FromSeconds(5), shutdown.Token);
    }
}
