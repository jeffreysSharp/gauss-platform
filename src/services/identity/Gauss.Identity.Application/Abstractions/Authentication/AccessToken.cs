namespace Gauss.Identity.Application.Abstractions.Authentication;

public sealed record AccessToken(
    string Value,
    string TokenType,
    DateTimeOffset ExpiresAtUtc);
