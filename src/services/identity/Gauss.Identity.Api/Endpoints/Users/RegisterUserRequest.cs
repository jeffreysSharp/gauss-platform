namespace Gauss.Identity.Api.Endpoints.Users;

public sealed record RegisterUserRequest(
    string Name,
    string Email,
    string Password);
