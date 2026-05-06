namespace Gauss.Identity.Infrastructure.Authentication;

public sealed record RefreshTokenOptions
{
    public const string SectionName = "Identity:RefreshToken";

    public const int MinimumExpirationMinutes = 1;

    public int ExpirationMinutes { get; init; } = 60 * 24 * 7;
}
