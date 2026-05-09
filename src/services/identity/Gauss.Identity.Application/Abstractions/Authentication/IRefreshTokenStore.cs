using Gauss.Identity.Application.Authentication.RefreshTokens;
using Gauss.Identity.Domain.RefreshTokens;

namespace Gauss.Identity.Application.Abstractions.Authentication;

public interface IRefreshTokenStore
{
    Task StoreAsync(
        RefreshTokenSession session,
        CancellationToken cancellationToken = default);

    Task<RefreshTokenSession?> GetByHashAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        RefreshTokenSession session,
        CancellationToken cancellationToken = default);

    Task RevokeFamilyAsync(
        Guid familyId,
        DateTimeOffset revokedAtUtc,
        CancellationToken cancellationToken = default);
}
