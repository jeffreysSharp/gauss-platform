using Gauss.Identity.Application.Authentication.Login;

namespace Gauss.Identity.Api.Endpoints.Authentication;

public static class LoginMapping
{
    public static LoginCommand ToCommand(
        this LoginRequest request)
    {
        return new LoginCommand(
            request.Email,
            request.Password);
    }
}
