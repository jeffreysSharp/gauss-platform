namespace Gauss.Identity.Application.Authentication.RefreshTokens;

public sealed record RefreshTokenResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc);
