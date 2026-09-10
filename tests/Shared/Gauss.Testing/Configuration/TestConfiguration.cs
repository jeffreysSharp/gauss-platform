namespace Gauss.Testing.Configuration;

public static class TestConfiguration
{
    private static readonly Lazy<Dictionary<string, string>> _dotEnvValues =
        new(LoadDotEnv);

    public static string? GetOptional(string key)
    {
        var envValue = Environment.GetEnvironmentVariable(key);
        if (envValue is not null)
            return envValue;

        _dotEnvValues.Value.TryGetValue(key, out var dotEnvValue);
        return dotEnvValue;
    }

    public static string GetRequired(string key)
    {
        return GetOptional(key)
            ?? throw new InvalidOperationException(
                $"Required test configuration '{key}' was not found. " +
                "Set it as an environment variable for the test process or in a .env file at the repository root.");
    }

    private static Dictionary<string, string> LoadDotEnv()
    {
        var repoRoot = FindRepositoryRoot();
        if (repoRoot is null)
            return [];

        var dotEnvPath = Path.Combine(repoRoot, ".env");
        if (!File.Exists(dotEnvPath))
            return [];

        var result = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var line in File.ReadLines(dotEnvPath))
        {
            var trimmed = line.Trim();

            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                continue;

            var separatorIndex = trimmed.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            var key = trimmed[..separatorIndex].Trim();
            var value = trimmed[(separatorIndex + 1)..].Trim();

            if (value.Length >= 2 && value.StartsWith('"') && value.EndsWith('"'))
                value = value[1..^1];

            result[key] = value;
        }

        return result;
    }

    private static string? FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (directory.EnumerateFiles("Gauss.slnx").Any() ||
                directory.EnumerateDirectories(".git").Any())
                return directory.FullName;

            directory = directory.Parent;
        }

        return null;
    }
}
