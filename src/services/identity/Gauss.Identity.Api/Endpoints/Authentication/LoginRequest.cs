namespace Gauss.Identity.Api.Endpoints.Authentication;

public sealed record LoginRequest(
    string Email,
    string Password);
