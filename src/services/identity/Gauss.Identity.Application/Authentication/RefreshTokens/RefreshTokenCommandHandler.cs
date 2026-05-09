using Gauss.BuildingBlocks.Application.Abstractions.Messaging;
using Gauss.BuildingBlocks.Application.Abstractions.Results;
using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Application.Abstractions.Time;
using Gauss.Identity.Domain.RefreshTokens;
using Gauss.Identity.Domain.Users;

namespace Gauss.Identity.Application.Authentication.RefreshTokens;

public sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IAccessTokenProvider accessTokenProvider,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenHasher refreshTokenHasher,
    IRefreshTokenStore refreshTokenStore,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    public async Task<Result<RefreshTokenResponse>> HandleAsync(
        RefreshTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var utcNow = dateTimeProvider.UtcNow;

        var refreshTokenHash = refreshTokenHasher.Hash(command.RefreshToken);

        var currentSession = await refreshTokenStore.GetByHashAsync(
            refreshTokenHash,
            cancellationToken);

        if (currentSession is null)
        {
            return Result<RefreshTokenResponse>.Failure(
                RefreshTokenErrors.InvalidToken);
        }

        if (currentSession.IsReusableAttackCandidate(utcNow))
        {
            var compromisedSession = currentSession.MarkReuseDetected(utcNow);

            await refreshTokenStore.UpdateAsync(
                compromisedSession,
                cancellationToken);

            await refreshTokenStore.RevokeFamilyAsync(
                compromisedSession.FamilyId,
                utcNow,
                cancellationToken);

            return Result<RefreshTokenResponse>.Failure(
                RefreshTokenErrors.InvalidToken);
        }

        if (!currentSession.IsActive(utcNow))
        {
            return Result<RefreshTokenResponse>.Failure(
                RefreshTokenErrors.InvalidToken);
        }

        var userId = UserId.From(currentSession.UserId);

        var user = await userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user is null)
        {
            return Result<RefreshTokenResponse>.Failure(
                RefreshTokenErrors.InvalidToken);
        }

        if (!user.CanAuthenticate(utcNow))
        {
            return Result<RefreshTokenResponse>.Failure(
                RefreshTokenErrors.UserUnavailable);
        }

        var accessToken = accessTokenProvider.Generate(user);

        var newRefreshToken = refreshTokenGenerator.Generate(utcNow);

        var newRefreshTokenHash = refreshTokenHasher.Hash(
            newRefreshToken.Value);

        var newSession = new RefreshTokenSession(
            SessionId: Guid.NewGuid(),
            FamilyId: currentSession.FamilyId,
            UserId: user.Id.Value,
            TenantId: user.TenantId.Value,
            RefreshTokenHash: newRefreshTokenHash,
            IssuedAtUtc: utcNow,
            ExpiresAtUtc: newRefreshToken.ExpiresAtUtc);

        await refreshTokenStore.StoreAsync(
            newSession,
            cancellationToken);

        var rotatedCurrentSession = currentSession.Rotate(
            newSession.SessionId,
            utcNow);

        await refreshTokenStore.UpdateAsync(
            rotatedCurrentSession,
            cancellationToken);

        var response = new RefreshTokenResponse(
            accessToken.Value,
            accessToken.TokenType,
            accessToken.ExpiresAtUtc,
            newRefreshToken.Value,
            newRefreshToken.ExpiresAtUtc);

        return Result<RefreshTokenResponse>.Success(response);
    }
}
