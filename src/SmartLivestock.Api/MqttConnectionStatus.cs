namespace SmartLivestock.Api;

public sealed class MqttConnectionStatus
{
    private volatile bool connected;

    public bool IsConnected
    {
        get => connected;
        set => connected = value;
    }
}
