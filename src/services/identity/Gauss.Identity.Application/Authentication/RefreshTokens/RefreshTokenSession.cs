namespace Gauss.Identity.Application.Authentication.RefreshTokens;

public sealed record RefreshTokenSession(
    Guid SessionId,
    Guid FamilyId,
    Guid UserId,
    Guid TenantId,
    string RefreshTokenHash,
    DateTimeOffset IssuedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    DateTimeOffset? RotatedAtUtc = null,
    DateTimeOffset? RevokedAtUtc = null,
    Guid? ReplacedBySessionId = null,
    DateTimeOffset? ReuseDetectedAtUtc = null)
{
    public bool IsExpired(DateTimeOffset utcNow)
    {
        return ExpiresAtUtc <= utcNow;
    }

    public bool IsRevoked => RevokedAtUtc.HasValue;

    public bool IsRotated => RotatedAtUtc.HasValue;

    public bool IsReuseDetected => ReuseDetectedAtUtc.HasValue;

    public bool IsActive(DateTimeOffset utcNow)
    {
        return !IsExpired(utcNow)
            && !IsRevoked
            && !IsRotated
            && !IsReuseDetected;
    }

    public bool IsReusableAttackCandidate(DateTimeOffset utcNow)
    {
        return !IsExpired(utcNow)
            && (IsRotated || IsRevoked || IsReuseDetected);
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

    public RefreshTokenSession MarkReuseDetected(DateTimeOffset reuseDetectedAtUtc)
    {
        return this with
        {
            ReuseDetectedAtUtc = reuseDetectedAtUtc,
            RevokedAtUtc = RevokedAtUtc ?? reuseDetectedAtUtc
        };
    }
}
