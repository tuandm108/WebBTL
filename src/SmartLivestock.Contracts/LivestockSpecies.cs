namespace SmartLivestock.Contracts;

public sealed record SpeciesOption(string Code, string Name);

/// <summary>Demo species catalog. These names identify animals; they do not define safety limits.</summary>
public static class LivestockSpecies
{
    public static IReadOnlyList<SpeciesOption> All { get; } =
    [
        new("cattle", "Bò"),
        new("buffalo", "Trâu"),
        new("pig", "Heo (lợn)"),
        new("chicken", "Gà"),
        new("duck", "Vịt"),
        new("muscovy_duck", "Ngan"),
        new("goose", "Ngỗng"),
        new("goat", "Dê"),
        new("sheep", "Cừu"),
        new("rabbit", "Thỏ"),
        new("quail", "Chim cút"),
        new("pigeon", "Bồ câu")
    ];

    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["cow"] = "cattle", ["bò"] = "cattle", ["trâu"] = "buffalo",
        ["heo"] = "pig", ["lợn"] = "pig", ["gà"] = "chicken",
        ["vịt"] = "duck", ["ngan"] = "muscovy_duck", ["ngỗng"] = "goose",
        ["dê"] = "goat", ["cừu"] = "sheep", ["thỏ"] = "rabbit",
        ["chim cút"] = "quail", ["bồ câu"] = "pigeon"
    };

    public static string? Normalize(string? value)
    {
        var text = value?.Trim();
        if (string.IsNullOrEmpty(text)) return null;
        var option = All.FirstOrDefault(x => x.Code.Equals(text, StringComparison.OrdinalIgnoreCase));
        return option?.Code ?? (Aliases.TryGetValue(text, out var code) ? code : null);
    }

    public static string NameOf(string? value)
    {
        var code = Normalize(value);
        return All.FirstOrDefault(x => x.Code == code)?.Name ?? value ?? "Chưa rõ loài";
    }
}
