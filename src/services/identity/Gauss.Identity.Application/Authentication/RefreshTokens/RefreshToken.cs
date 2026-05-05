namespace Gauss.Identity.Application.Authentication.RefreshTokens;

public sealed record RefreshToken(
    string Value,
    DateTimeOffset ExpiresAtUtc);
