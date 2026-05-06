using AwesomeAssertions;
using Gauss.Identity.Infrastructure.Authentication;

namespace Gauss.Identity.InfrastructureTests.Authentication;

public sealed class Sha256RefreshTokenHasherTests
{
    [Fact(DisplayName = "Should hash refresh token")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Hash_Refresh_Token()
    {
        // Arrange
        var hasher = new Sha256RefreshTokenHasher();

        const string refreshToken = "refresh-token-value";

        // Act
        var hash = hasher.Hash(refreshToken);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe(refreshToken);
        hash.Should().HaveLength(64);
    }

    [Fact(DisplayName = "Should generate same hash for same refresh token")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Generate_Same_Hash_For_Same_Refresh_Token()
    {
        // Arrange
        var hasher = new Sha256RefreshTokenHasher();

        const string refreshToken = "refresh-token-value";

        // Act
        var firstHash = hasher.Hash(refreshToken);
        var secondHash = hasher.Hash(refreshToken);

        // Assert
        firstHash.Should().Be(secondHash);
    }

    [Fact(DisplayName = "Should verify refresh token when hash matches")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Verify_Refresh_Token_When_Hash_Matches()
    {
        // Arrange
        var hasher = new Sha256RefreshTokenHasher();

        const string refreshToken = "refresh-token-value";
        var refreshTokenHash = hasher.Hash(refreshToken);

        // Act
        var isValid = hasher.Verify(
            refreshToken,
            refreshTokenHash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Should not verify refresh token when hash does not match")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Not_Verify_Refresh_Token_When_Hash_Does_Not_Match()
    {
        // Arrange
        var hasher = new Sha256RefreshTokenHasher();

        const string refreshToken = "refresh-token-value";
        const string otherRefreshToken = "other-refresh-token-value";

        var refreshTokenHash = hasher.Hash(otherRefreshToken);

        // Act
        var isValid = hasher.Verify(
            refreshToken,
            refreshTokenHash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact(DisplayName = "Should throw argument exception when refresh token is empty")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Throw_ArgumentException_When_RefreshToken_Is_Empty()
    {
        // Arrange
        var hasher = new Sha256RefreshTokenHasher();

        // Act
        var action = () => hasher.Hash(string.Empty);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Should throw argument exception when refresh token hash is empty")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Throw_ArgumentException_When_RefreshTokenHash_Is_Empty()
    {
        // Arrange
        var hasher = new Sha256RefreshTokenHasher();

        // Act
        var action = () => hasher.Verify(
            "refresh-token-value",
            string.Empty);

        // Assert
        action.Should().Throw<ArgumentException>();
    }
}
