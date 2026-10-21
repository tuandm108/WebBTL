namespace SmartLivestock.Contracts;

public sealed record LocalAuthSettings(string ApiKey, string? AdminEmail, string? AdminPassword)
{
    public static void RemoveBootstrapPassword()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null; directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, "deploy", "native", "auth.env");
            if (!File.Exists(path)) continue;
            File.WriteAllLines(path, File.ReadAllLines(path)
                .Where(line => !line.TrimStart('\uFEFF').StartsWith("ADMIN_PASSWORD=", StringComparison.Ordinal)));
            return;
        }
    }

    public static LocalAuthSettings? TryLoad()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null; directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, "deploy", "native", "auth.env");
            if (!File.Exists(path)) continue;
            var values = File.ReadLines(path)
                .Select(line => line.Split('=', 2))
                .Where(parts => parts.Length == 2)
                .ToDictionary(parts => parts[0].Trim().TrimStart('\uFEFF'), parts => parts[1].Trim());
            if (!values.TryGetValue("AUTH_API_KEY", out var key) || key.Length < 32)
                throw new InvalidOperationException("AUTH_API_KEY must be at least 32 characters");
            values.TryGetValue("ADMIN_EMAIL", out var email);
            values.TryGetValue("ADMIN_PASSWORD", out var password);
            return new(key, email, password);
        }
        return null;
    }
}
