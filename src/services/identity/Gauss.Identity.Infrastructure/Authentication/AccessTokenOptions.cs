namespace Gauss.Identity.Infrastructure.Authentication;

public sealed record AccessTokenOptions
{
    public const string SectionName = "Identity:AccessToken";

    public const int MinimumSecretKeyLength = 32;

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string SecretKey { get; init; } = string.Empty;

    public int ExpirationMinutes { get; init; } = 15;
}
