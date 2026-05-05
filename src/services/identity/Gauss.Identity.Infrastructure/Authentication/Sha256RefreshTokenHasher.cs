using System.Security.Cryptography;
using System.Text;
using Gauss.Identity.Application.Abstractions.Authentication;

namespace Gauss.Identity.Infrastructure.Authentication;

public sealed class Sha256RefreshTokenHasher : IRefreshTokenHasher
{
    public string Hash(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        var bytes = Encoding.UTF8.GetBytes(refreshToken);
        var hashBytes = SHA256.HashData(bytes);

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public bool Verify(
        string refreshToken,
        string refreshTokenHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshTokenHash);

        var computedHash = Hash(refreshToken);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(refreshTokenHash));
    }
}
