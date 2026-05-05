namespace Gauss.Identity.Infrastructure.Persistence;

public sealed record RedisOptions
{
    public const string SectionName = "Identity:Redis";

    public string ConnectionString { get; init; } = string.Empty;
}
