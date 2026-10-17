using Microsoft.AspNetCore.SignalR;

namespace SmartLivestock.Api;

public sealed class MonitoringHub : Hub
{
    public const string AlertsGroup = "alerts";

    // Every client gets alert events; telemetry stays opt-in per barn.
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, AlertsGroup);
        await base.OnConnectedAsync();
    }

    public Task SubscribeBarn(string barnId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, ParseBarn(barnId));

    public Task UnsubscribeBarn(string barnId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, ParseBarn(barnId));

    private static string ParseBarn(string barnId) =>
        Guid.TryParse(barnId, out var id) ? id.ToString("D") : throw new HubException("Invalid barn ID");
}
