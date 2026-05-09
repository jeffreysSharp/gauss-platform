using Gauss.BuildingBlocks.Application.Abstractions.Results;

namespace Gauss.Identity.Application.Authentication.Login;

public static class LoginErrors
{
    public static readonly Error InvalidCredentials = Error.Unauthorized(
        "Identity.Login.InvalidCredentials",
        "Invalid email or password.");

    public static readonly Error UserUnavailable = Error.Forbidden(
        "Identity.Login.UserUnavailable",
        "The user is not available for authentication.");
}
