using Gauss.BuildingBlocks.Application.Abstractions.Messaging;
using Gauss.BuildingBlocks.Application.Abstractions.Results;
using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Application.Abstractions.Time;
using Gauss.Identity.Domain.Users.ValueObjects;

namespace Gauss.Identity.Application.Authentication.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IAccessTokenProvider accessTokenProvider,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<LoginCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Email email;

        try
        {
            email = Email.Create(command.Email);
        }
        catch (ArgumentException)
        {
            return Result<LoginResponse>.Failure(LoginErrors.InvalidEmail);
        }

        var user = await userRepository.GetByEmailAsync(
            email,
            cancellationToken);

        if (user is null)
        {
            return Result<LoginResponse>.Failure(LoginErrors.InvalidCredentials);
        }

        var passwordVerificationStatus = passwordHasher.Verify(
            user.PasswordHash,
            command.Password);

        if (passwordVerificationStatus == PasswordVerificationStatus.Failed)
        {
            return Result<LoginResponse>.Failure(LoginErrors.InvalidCredentials);
        }

        if (!user.CanAuthenticate(dateTimeProvider.UtcNow))
        {
            return Result<LoginResponse>.Failure(LoginErrors.UserUnavailable);
        }

        user.RegisterSuccessfulLogin(dateTimeProvider.UtcNow);

        await userRepository.UpdateLastLoginAsync(
            user,
            cancellationToken);

        var accessToken = accessTokenProvider.Generate(user);

        var response = new LoginResponse(
            user.Id.Value,
            user.TenantId.Value,
            user.Name,
            user.Email.Value,
            accessToken.Value,
            accessToken.TokenType,
            accessToken.ExpiresAtUtc);

        return Result<LoginResponse>.Success(response);
    }
}
