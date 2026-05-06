using AwesomeAssertions;
using Gauss.Identity.Infrastructure.Authentication;

namespace Gauss.Identity.InfrastructureTests.Authentication;

public sealed class RefreshTokenOptionsValidatorTests
{
    [Fact(DisplayName = "Should return success when refresh token options are valid")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Return_Success_When_RefreshTokenOptions_Are_Valid()
    {
        // Arrange
        var validator = new RefreshTokenOptionsValidator();

        var options = new RefreshTokenOptions
        {
            ExpirationMinutes = 10080
        };

        // Act
        var result = validator.Validate(
            name: null,
            options);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Failed.Should().BeFalse();
        result.Failures.Should().BeNull();
    }

    [Theory(DisplayName = "Should fail when refresh token expiration minutes is invalid")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Fail_When_ExpirationMinutes_Is_Invalid(
        int expirationMinutes)
    {
        // Arrange
        var validator = new RefreshTokenOptionsValidator();

        var options = new RefreshTokenOptions
        {
            ExpirationMinutes = expirationMinutes
        };

        // Act
        var result = validator.Validate(
            name: null,
            options);

        // Assert
        result.Failed.Should().BeTrue();

        result.Failures.Should().Contain(
            $"Refresh token expiration must be greater than or equal to {RefreshTokenOptions.MinimumExpirationMinutes} minute.");
    }
}
