using Gauss.Identity.Application.Authentication.RefreshTokens;

namespace Gauss.Identity.Api.Endpoints.Authentication;

public static class RefreshTokenMapping
{
    public static RefreshTokenCommand ToCommand(
        this RefreshTokenRequest request)
    {
        return new RefreshTokenCommand(
            request.RefreshToken);
    }
}
