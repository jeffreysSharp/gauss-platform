using AwesomeAssertions;
using Gauss.Identity.Application.Authentication.RefreshTokens;

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

    private static RefreshTokenSession CreateSession(
        DateTimeOffset issuedAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        return new RefreshTokenSession(
            SessionId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            RefreshTokenHash: "refresh-token-hash",
            IssuedAtUtc: issuedAtUtc,
            ExpiresAtUtc: expiresAtUtc);
    }
}
