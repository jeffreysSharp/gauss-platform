namespace Gauss.Identity.Api.Endpoints.Authentication;

public sealed record CurrentUserResponse(
    Guid UserId,
    Guid TenantId,
    string Name,
    string Email);
