namespace Gauss.Identity.Application.Abstractions.Authentication;

public sealed record RefreshToken(
    string Value,
    DateTimeOffset ExpiresAtUtc);
