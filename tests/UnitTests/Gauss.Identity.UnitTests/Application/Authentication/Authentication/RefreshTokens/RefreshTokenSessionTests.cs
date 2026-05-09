using AwesomeAssertions;
using Gauss.Identity.Domain.RefreshTokens;

namespace Gauss.Identity.UnitTests.Application.Authentication.Authentication.RefreshTokens;

public sealed class RefreshTokenSessionTests
{
    [Fact(DisplayName = "Should be active when session is not expired revoked or rotated")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Authentication")]
    public void Should_Be_Active_When_Session_Is_Not_Expired_Revoked_Or_Rotated()
    {
        // Arrange
        var utcNow = new DateTimeOffset(2026, 05, 04, 12, 0, 0, TimeSpan.Zero);

        var session = CreateSession(
            issuedAtUtc: utcNow.AddMinutes(-5),
            expiresAtUtc: utcNow.AddMinutes(10));

        // Act
        var isActive = session.IsActive(utcNow);

        // Assert
        isActive.Should().BeTrue();
    }

    [Fact(DisplayName = "Should not be active when session is expired")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Authentication")]
    public void Should_Not_Be_Active_When_Session_Is_Expired()
    {
        // Arrange
        var utcNow = new DateTimeOffset(2026, 05, 04, 12, 0, 0, TimeSpan.Zero);

        var session = CreateSession(
            issuedAtUtc: utcNow.AddMinutes(-20),
            expiresAtUtc: utcNow);

        // Act
        var isActive = session.IsActive(utcNow);

        // Assert
        isActive.Should().BeFalse();
        session.IsExpired(utcNow).Should().BeTrue();
    }

    [Fact(DisplayName = "Should not be active when session is revoked")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Authentication")]
    public void Should_Not_Be_Active_When_Session_Is_Revoked()
    {
        // Arrange
        var utcNow = new DateTimeOffset(2026, 05, 04, 12, 0, 0, TimeSpan.Zero);

        var session = CreateSession(
            issuedAtUtc: utcNow.AddMinutes(-5),
            expiresAtUtc: utcNow.AddMinutes(10))
            .Revoke(utcNow);

        // Act
        var isActive = session.IsActive(utcNow);

        // Assert
        isActive.Should().BeFalse();
        session.IsRevoked.Should().BeTrue();
        session.RevokedAtUtc.Should().Be(utcNow);
    }

    [Fact(DisplayName = "Should not be active when session is rotated")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Authentication")]
    public void Should_Not_Be_Active_When_Session_Is_Rotated()
    {
        // Arrange
        var utcNow = new DateTimeOffset(2026, 05, 04, 12, 0, 0, TimeSpan.Zero);
        var replacedBySessionId = Guid.NewGuid();

        var session = CreateSession(
            issuedAtUtc: utcNow.AddMinutes(-5),
            expiresAtUtc: utcNow.AddMinutes(10))
            .Rotate(
                replacedBySessionId,
                utcNow);

        // Act
        var isActive = session.IsActive(utcNow);

        // Assert
        isActive.Should().BeFalse();
        session.IsRotated.Should().BeTrue();
        session.RotatedAtUtc.Should().Be(utcNow);
        session.ReplacedBySessionId.Should().Be(replacedBySessionId);
    }

    [Fact(DisplayName = "Should not be active when reuse is detected")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Authentication")]
    public void Should_Not_Be_Active_When_Reuse_Is_Detected()
    {
        // Arrange
        var utcNow = new DateTimeOffset(2026, 05, 07, 12, 0, 0, TimeSpan.Zero);

        var session = CreateSession(
            issuedAtUtc: utcNow.AddMinutes(-5),
            expiresAtUtc: utcNow.AddMinutes(10))
            .MarkReuseDetected(utcNow);

        // Act
        var isActive = session.IsActive(utcNow);

        // Assert
        isActive.Should().BeFalse();
        session.IsReuseDetected.Should().BeTrue();
        session.ReuseDetectedAtUtc.Should().Be(utcNow);
        session.RevokedAtUtc.Should().Be(utcNow);
    }

    [Fact(DisplayName = "Should identify reusable attack candidate when rotated session is reused before expiration")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Authentication")]
    public void Should_Identify_Reusable_Attack_Candidate_When_Rotated_Session_Is_Reused_Before_Expiration()
    {
        // Arrange
        var utcNow = new DateTimeOffset(2026, 05, 07, 12, 0, 0, TimeSpan.Zero);

        var session = CreateSession(
            issuedAtUtc: utcNow.AddMinutes(-5),
            expiresAtUtc: utcNow.AddMinutes(10))
            .Rotate(
                Guid.NewGuid(),
                utcNow);

        // Act
        var isReusableAttackCandidate = session.IsReusableAttackCandidate(utcNow);

        // Assert
        isReusableAttackCandidate.Should().BeTrue();
    }

    [Fact(DisplayName = "Should not identify reusable attack candidate when rotated session is expired")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Authentication")]
    public void Should_Not_Identify_Reusable_Attack_Candidate_When_Rotated_Session_Is_Expired()
    {
        // Arrange
        var utcNow = new DateTimeOffset(2026, 05, 07, 12, 0, 0, TimeSpan.Zero);

        var session = CreateSession(
            issuedAtUtc: utcNow.AddDays(-8),
            expiresAtUtc: utcNow.AddMinutes(-1))
            .Rotate(
                Guid.NewGuid(),
                utcNow.AddDays(-1));

        // Act
        var isReusableAttackCandidate = session.IsReusableAttackCandidate(utcNow);

        // Assert
        isReusableAttackCandidate.Should().BeFalse();
    }

    private static RefreshTokenSession CreateSession(
    DateTimeOffset issuedAtUtc,
    DateTimeOffset expiresAtUtc)
    {
        return new RefreshTokenSession(
            SessionId: Guid.NewGuid(),
            FamilyId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            RefreshTokenHash: "refresh-token-hash",
            IssuedAtUtc: issuedAtUtc,
            ExpiresAtUtc: expiresAtUtc);
    }
}
