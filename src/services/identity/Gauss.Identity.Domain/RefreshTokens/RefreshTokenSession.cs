namespace Gauss.Identity.Domain.RefreshTokens;

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
    public static RefreshTokenSession CreateNewFamily(
        Guid userId,
        Guid tenantId,
        string refreshTokenHash,
        DateTimeOffset issuedAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        return Create(
            Guid.NewGuid(),
            userId,
            tenantId,
            refreshTokenHash,
            issuedAtUtc,
            expiresAtUtc);
    }

    public static RefreshTokenSession CreateFromExistingFamily(
        Guid familyId,
        Guid userId,
        Guid tenantId,
        string refreshTokenHash,
        DateTimeOffset issuedAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        return Create(
            familyId,
            userId,
            tenantId,
            refreshTokenHash,
            issuedAtUtc,
            expiresAtUtc);
    }

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
            RevokedAtUtc = RevokedAtUtc ?? revokedAtUtc
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

    private static RefreshTokenSession Create(
        Guid familyId,
        Guid userId,
        Guid tenantId,
        string refreshTokenHash,
        DateTimeOffset issuedAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        return new RefreshTokenSession(
            SessionId: Guid.NewGuid(),
            FamilyId: familyId,
            UserId: userId,
            TenantId: tenantId,
            RefreshTokenHash: refreshTokenHash,
            IssuedAtUtc: issuedAtUtc,
            ExpiresAtUtc: expiresAtUtc);
    }
}
