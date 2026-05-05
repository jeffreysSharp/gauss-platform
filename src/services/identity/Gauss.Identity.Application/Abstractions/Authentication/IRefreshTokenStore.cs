using Gauss.Identity.Application.Authentication.RefreshTokens;

namespace Gauss.Identity.Application.Abstractions.Authentication;

public interface IRefreshTokenStore
{
    Task StoreAsync(
        RefreshTokenSession session,
        CancellationToken cancellationToken = default);

    Task<RefreshTokenSession?> GetByHashAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default);
}
