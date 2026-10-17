using Microsoft.JSInterop;

namespace SmartLivestock.Web.Services;

/// <summary>Remembers the chosen rows-per-page in the browser, one key per table. Needs an interactive circuit.</summary>
public sealed class PageSizeStore(IJSRuntime js)
{
    public async Task<int> LoadAsync(string key, int fallback, int max)
    {
        try
        {
            var raw = await js.InvokeAsync<string?>("localStorage.getItem", key);
            return int.TryParse(raw, out var size) ? Math.Clamp(size, 1, max) : fallback;
        }
        catch (Exception) { return fallback; } // storage blocked: keep the default
    }

    public async Task SaveAsync(string key, int size)
    {
        try { await js.InvokeVoidAsync("localStorage.setItem", key, size.ToString()); }
        catch (Exception) { }
    }
}
