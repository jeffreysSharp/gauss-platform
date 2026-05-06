using AwesomeAssertions;
using Gauss.Identity.Infrastructure.Authentication;
using Microsoft.Extensions.Options;

namespace Gauss.Identity.InfrastructureTests.Authentication;

public sealed class SecureRefreshTokenGeneratorTests
{
    [Fact(DisplayName = "Should generate opaque refresh token")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Generate_Opaque_Refresh_Token()
    {
        // Arrange
        var issuedAtUtc = new DateTimeOffset(2026, 05, 04, 12, 0, 0, TimeSpan.Zero);

        var generator = new SecureRefreshTokenGenerator(
            Options.Create(new RefreshTokenOptions
            {
                ExpirationMinutes = 10080
            }));

        // Act
        var refreshToken = generator.Generate(issuedAtUtc);

        // Assert
        refreshToken.Value.Should().NotBeNullOrWhiteSpace();
        refreshToken.Value.Should().NotContain(".");
        refreshToken.ExpiresAtUtc.Should().Be(issuedAtUtc.AddMinutes(10080));
    }

    [Fact(DisplayName = "Should generate different refresh tokens")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Generate_Different_Refresh_Tokens()
    {
        // Arrange
        var issuedAtUtc = new DateTimeOffset(2026, 05, 04, 12, 0, 0, TimeSpan.Zero);

        var generator = new SecureRefreshTokenGenerator(
            Options.Create(new RefreshTokenOptions
            {
                ExpirationMinutes = 10080
            }));

        // Act
        var firstRefreshToken = generator.Generate(issuedAtUtc);
        var secondRefreshToken = generator.Generate(issuedAtUtc);

        // Assert
        firstRefreshToken.Value.Should().NotBe(secondRefreshToken.Value);
    }

    [Fact(DisplayName = "Should generate URL safe refresh token")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Generate_Url_Safe_Refresh_Token()
    {
        // Arrange
        var issuedAtUtc = new DateTimeOffset(2026, 05, 04, 12, 0, 0, TimeSpan.Zero);

        var generator = new SecureRefreshTokenGenerator(
            Options.Create(new RefreshTokenOptions
            {
                ExpirationMinutes = 10080
            }));

        // Act
        var refreshToken = generator.Generate(issuedAtUtc);

        // Assert
        refreshToken.Value
            .ToCharArray()
            .Should()
            .OnlyContain(character =>
                char.IsLetterOrDigit(character) ||
                character == '-' ||
                character == '_');
    }
}
