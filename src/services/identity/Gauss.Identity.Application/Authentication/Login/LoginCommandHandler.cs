using Gauss.BuildingBlocks.Application.Abstractions.Messaging;
using Gauss.BuildingBlocks.Application.Abstractions.Results;
using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Application.Abstractions.Time;
using Gauss.Identity.Domain.RefreshTokens;
using Gauss.Identity.Domain.Users.ValueObjects;

namespace Gauss.Identity.Application.Authentication.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IAccessTokenProvider accessTokenProvider,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenHasher refreshTokenHasher,
    IRefreshTokenStore refreshTokenStore,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<LoginCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!Email.TryCreate(command.Email, out var email))
        {
            return Result<LoginResponse>.Failure(LoginErrors.InvalidCredentials);
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

        var utcNow = dateTimeProvider.UtcNow;

        if (!user.CanAuthenticate(utcNow))
        {
            return Result<LoginResponse>.Failure(LoginErrors.UserUnavailable);
        }

        user.RegisterSuccessfulLogin(utcNow);

        await userRepository.RecordLoginAsync(
            user.Id,
            utcNow,
            cancellationToken);

        var accessToken = accessTokenProvider.Generate(user);

        var refreshToken = refreshTokenGenerator.Generate(utcNow);

        var refreshTokenHash = refreshTokenHasher.Hash(refreshToken.Value);

        var refreshTokenSession = new RefreshTokenSession(
            SessionId: Guid.NewGuid(),
            FamilyId: Guid.NewGuid(),
            UserId: user.Id.Value,
            TenantId: user.TenantId.Value,
            RefreshTokenHash: refreshTokenHash,
            IssuedAtUtc: utcNow,
            ExpiresAtUtc: refreshToken.ExpiresAtUtc);

        await refreshTokenStore.StoreAsync(
            refreshTokenSession,
            cancellationToken);

        var response = new LoginResponse(
            user.Id.Value,
            user.TenantId.Value,
            user.Name,
            user.Email.Value,
            accessToken.Value,
            accessToken.TokenType,
            accessToken.ExpiresAtUtc,
            refreshToken.Value,
            refreshToken.ExpiresAtUtc);

        return Result<LoginResponse>.Success(response);
    }
}
