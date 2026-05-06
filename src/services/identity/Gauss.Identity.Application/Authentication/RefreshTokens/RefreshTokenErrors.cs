using Gauss.BuildingBlocks.Application.Abstractions.Results;


namespace Gauss.Identity.Application.Authentication.RefreshTokens;

public static class RefreshTokenErrors
{
    public static readonly Error InvalidToken = Error.Unauthorized(
        "Identity.RefreshToken.InvalidToken",
        "Invalid refresh token.");

    public static readonly Error UserUnavailable = Error.Forbidden(
        "Identity.RefreshToken.UserUnavailable",
        "The user is not available for authentication.");
}
