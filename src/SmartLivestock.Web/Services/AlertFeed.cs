using Microsoft.AspNetCore.SignalR.Client;
using SmartLivestock.Contracts;

namespace SmartLivestock.Web.Services;

/// <summary>
/// One alert-stream connection per browser circuit, shared by the alert dialog, the nav badge and the history page.
/// Handlers run on a SignalR thread, so subscribers must marshal to their own dispatcher.
/// </summary>
public sealed class AlertFeed(IHttpClientFactory httpFactory, IConfiguration config) : IAsyncDisposable
{
    private HubConnection? hub;

    public int ActiveCount { get; private set; }

    /// <summary>Alerts nobody has marked as read yet (any state); the nav badge shows this.</summary>
    public int UnreadCount { get; private set; }

    /// <summary>Raised after any alert change or reconnect, once <see cref="ActiveCount"/> is fresh.</summary>
    public event Action? Changed;

    /// <summary>Raised for every pushed alert change, before <see cref="Changed"/>.</summary>
    public event Action<AlertChangedDto>? Live;

    public async Task StartAsync()
    {
        if (hub is not null) return;
        var apiBase = config["Api:BaseUrl"] ?? "http://localhost:5215";
        var connection = new HubConnectionBuilder()
            .WithUrl($"{apiBase.TrimEnd('/')}/hubs/monitoring")
            .WithAutomaticReconnect()
            .Build();
        connection.On<AlertChangedDto>("alertChanged", async change =>
        {
            await RefreshCountAsync();
            Live?.Invoke(change);
            Changed?.Invoke();
        });
        // Another browser (or this one) marked alerts read: refresh the counters and let pages reload.
        connection.On<int>("alertsRead", async _ =>
        {
            await RefreshCountAsync();
            Changed?.Invoke();
        });
        connection.Reconnected += async _ =>
        {
            await RefreshCountAsync();
            Changed?.Invoke();
        };
        try
        {
            await connection.StartAsync();
            hub = connection;
            await RefreshCountAsync();
            Changed?.Invoke();
        }
        catch
        {
            await connection.DisposeAsync(); // API not up yet; the next page load tries again
        }
    }

    private async Task RefreshCountAsync()
    {
        try
        {
            var counts = await httpFactory.CreateClient("api").GetFromJsonAsync<AlertCountsDto>("api/alerts/active-count");
            if (counts is null) return;
            ActiveCount = counts.Total;
            UnreadCount = counts.Unread;
        }
        catch (HttpRequestException) { /* keep the last known counts */ }
    }

    public async ValueTask DisposeAsync()
    {
        if (hub is not null) await hub.DisposeAsync();
    }
}
