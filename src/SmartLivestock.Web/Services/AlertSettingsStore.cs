using System.Globalization;
using System.Text.Json;
using Microsoft.JSInterop;
using SmartLivestock.Contracts;

namespace SmartLivestock.Web.Services;

/// <summary>
/// Alert display preferences and the last scheduled run, kept in the browser's localStorage (there are no user
/// accounts yet). Needs an interactive circuit: call <see cref="LoadAsync"/> from OnAfterRenderAsync.
/// </summary>
public sealed class AlertSettingsStore(IJSRuntime js)
{
    private const string SettingsKey = "sl.alerts.settings";
    private const string LastRunKey = "sl.alerts.lastRun";
    private bool loaded;

    public AlertDisplaySettings Current { get; private set; } = AlertDisplaySchedule.Normalize(null);

    /// <summary>When the digest last ran on schedule (or when the schedule was first started).</summary>
    public DateTimeOffset? LastRun { get; private set; }

    public event Action? Changed;

    /// <summary>"Show it now" button on the settings page; handled by the alert dialog.</summary>
    public event Action? PreviewRequested;

    public async Task LoadAsync()
    {
        if (loaded) return;
        try
        {
            var raw = await js.InvokeAsync<string?>("localStorage.getItem", SettingsKey);
            if (raw is not null)
                Current = AlertDisplaySchedule.Normalize(JsonSerializer.Deserialize<AlertDisplaySettings>(raw));
            var run = await js.InvokeAsync<string?>("localStorage.getItem", LastRunKey);
            if (DateTimeOffset.TryParse(run, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
                LastRun = parsed.ToLocalTime();
        }
        catch (Exception) { /* storage blocked or corrupt JSON: keep the defaults */ }
        loaded = true;
        Changed?.Invoke();
    }

    public async Task SaveAsync(AlertDisplaySettings value)
    {
        Current = AlertDisplaySchedule.Normalize(value);
        try { await js.InvokeVoidAsync("localStorage.setItem", SettingsKey, JsonSerializer.Serialize(Current)); }
        catch (Exception) { }
        Changed?.Invoke();
    }

    public async Task SetLastRunAsync(DateTimeOffset value)
    {
        LastRun = value.ToLocalTime();
        try { await js.InvokeVoidAsync("localStorage.setItem", LastRunKey, value.ToString("O")); }
        catch (Exception) { }
        Changed?.Invoke();
    }

    public void RequestPreview() => PreviewRequested?.Invoke();
}
