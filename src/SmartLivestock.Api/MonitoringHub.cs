using Microsoft.AspNetCore.SignalR;

namespace SmartLivestock.Api;

public sealed class MonitoringHub : Hub
{
    public Task SubscribeBarn(string barnId)
    {
        if (!Guid.TryParse(barnId, out var id)) throw new HubException("Invalid barn ID");
        return Groups.AddToGroupAsync(Context.ConnectionId, id.ToString("D"));
    }
}
