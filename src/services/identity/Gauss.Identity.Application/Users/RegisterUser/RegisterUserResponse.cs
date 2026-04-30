namespace Gauss.Identity.Application.Users.RegisterUser;

public sealed record RegisterUserResponse(
    Guid UserId,
    Guid TenantId,
    string Name,
    string Email);
