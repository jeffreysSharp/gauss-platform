using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Gauss.Identity.Application.Abstractions.Authentication;

namespace Gauss.Identity.Api.Authentication;

public sealed class HttpCurrentUserContext(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentUserContext
{
    private ClaimsPrincipal? User =>
        httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;

    public Guid? UserId =>
        TryGetGuidClaim(JwtRegisteredClaimNames.Sub);

    public Guid? TenantId =>
        TryGetGuidClaim(GaussClaimTypes.TenantId);

    public string? Name =>
        User?.FindFirstValue(JwtRegisteredClaimNames.Name);

    public string? Email =>
        User?.FindFirstValue(JwtRegisteredClaimNames.Email);

    private Guid? TryGetGuidClaim(string claimType)
    {
        var value = User?.FindFirstValue(claimType);

        return Guid.TryParse(value, out var parsedValue)
            ? parsedValue
            : null;
    }
}
