namespace Gauss.Identity.Application.Authentication.RefreshTokens;

public sealed record RefreshTokenSession(
    Guid SessionId,
    Guid UserId,
    Guid TenantId,
    string RefreshTokenHash,
    DateTimeOffset IssuedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    DateTimeOffset? RotatedAtUtc = null,
    DateTimeOffset? RevokedAtUtc = null,
    Guid? ReplacedBySessionId = null)
{
    public bool IsExpired(DateTimeOffset utcNow)
    {
        return ExpiresAtUtc <= utcNow;
    }

    public bool IsRevoked => RevokedAtUtc.HasValue;

    public bool IsRotated => RotatedAtUtc.HasValue;

    public bool IsActive(DateTimeOffset utcNow)
    {
        return !IsExpired(utcNow)
            && !IsRevoked
            && !IsRotated;
    }

    public RefreshTokenSession Rotate(
        Guid replacedBySessionId,
        DateTimeOffset rotatedAtUtc)
    {
        return this with
        {
            RotatedAtUtc = rotatedAtUtc,
            ReplacedBySessionId = replacedBySessionId
        };
    }

    public RefreshTokenSession Revoke(DateTimeOffset revokedAtUtc)
    {
        return this with
        {
            RevokedAtUtc = revokedAtUtc
        };
    }
}
