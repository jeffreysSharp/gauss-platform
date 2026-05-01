using Gauss.Identity.Application.Users.RegisterUser;

namespace Gauss.Identity.Api.Endpoints.Users;

public static class RegisterUserMapping
{
    public static RegisterUserCommand ToCommand(
        this RegisterUserRequest request)
    {
        return new RegisterUserCommand(
            request.Name,
            request.Email,
            request.Password);
    }
}
