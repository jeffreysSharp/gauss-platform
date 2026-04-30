namespace Gauss.Identity.Infrastructure.Persistence;

public sealed class IdentityPersistenceOptions
{
    public const string SectionName = "Identity:Persistence";

    public string ConnectionString { get; init; } = string.Empty;
}
