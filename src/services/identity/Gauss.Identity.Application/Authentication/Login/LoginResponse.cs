namespace Gauss.Identity.Application.Authentication.Login;

public sealed record LoginResponse(
    Guid UserId,
    Guid TenantId,
    string Name,
    string Email,
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAtUtc);
