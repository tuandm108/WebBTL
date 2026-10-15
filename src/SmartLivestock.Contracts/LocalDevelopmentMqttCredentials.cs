namespace SmartLivestock.Contracts;

public sealed record LocalDevelopmentMqttCredentials(string User, string Password)
{
    public static LocalDevelopmentMqttCredentials? TryLoad()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, "deploy", "native", ".env");
            if (!File.Exists(path)) continue;

            string? user = null;
            string? password = null;
            foreach (var line in File.ReadLines(path))
            {
                var separator = line.IndexOf('=');
                if (separator <= 0) continue;
                var key = line[..separator].Trim();
                var value = line[(separator + 1)..].Trim();
                if (key == "MQTT_USER") user = value;
                if (key == "MQTT_PASSWORD") password = value;
            }

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException($"MQTT credentials are missing from {path}");

            return new LocalDevelopmentMqttCredentials(user, password);
        }

        return null;
    }
}
