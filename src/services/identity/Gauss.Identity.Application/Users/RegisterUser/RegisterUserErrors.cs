using Gauss.BuildingBlocks.Application.Abstractions.Results;

namespace Gauss.Identity.Application.Users.RegisterUser;

public static class RegisterUserErrors
{
    public static readonly Error EmailAlreadyExists = Error.Conflict(
        "Identity.User.EmailAlreadyExists",
        "A user with the specified email already exists.");

    public static readonly Error InvalidEmail = Error.Validation(
        "Identity.User.EmailInvalid",
        "The specified email is invalid.");
}
