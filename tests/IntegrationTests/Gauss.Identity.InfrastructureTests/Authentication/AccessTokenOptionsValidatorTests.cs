using AwesomeAssertions;
using Gauss.Identity.Infrastructure.Authentication;

namespace Gauss.Identity.InfrastructureTests.Authentication;

public sealed class AccessTokenOptionsValidatorTests
{
    [Fact(DisplayName = "Should return success when access token options are valid")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Return_Success_When_AccessTokenOptions_Are_Valid()
    {
        // Arrange
        var validator = new AccessTokenOptionsValidator();

        var options = CreateValidOptions();

        // Act
        var result = validator.Validate(
            name: null,
            options);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Failed.Should().BeFalse();
        result.Failures.Should().BeNull();
    }

    [Fact(DisplayName = "Should fail when issuer is not configured")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Fail_When_Issuer_Is_Not_Configured()
    {
        // Arrange
        var validator = new AccessTokenOptionsValidator();

        var options = CreateValidOptions() with
        {
            Issuer = string.Empty
        };

        // Act
        var result = validator.Validate(
            name: null,
            options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain("Access token issuer was not configured.");
    }

    [Fact(DisplayName = "Should fail when audience is not configured")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Fail_When_Audience_Is_Not_Configured()
    {
        // Arrange
        var validator = new AccessTokenOptionsValidator();

        var options = CreateValidOptions() with
        {
            Audience = string.Empty
        };

        // Act
        var result = validator.Validate(
            name: null,
            options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain("Access token audience was not configured.");
    }

    [Fact(DisplayName = "Should fail when secret key is not configured")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Fail_When_SecretKey_Is_Not_Configured()
    {
        // Arrange
        var validator = new AccessTokenOptionsValidator();

        var options = CreateValidOptions() with
        {
            SecretKey = string.Empty
        };

        // Act
        var result = validator.Validate(
            name: null,
            options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain("Access token secret key was not configured.");
    }

    [Fact(DisplayName = "Should fail when secret key is too short")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Fail_When_SecretKey_Is_Too_Short()
    {
        // Arrange
        var validator = new AccessTokenOptionsValidator();

        var options = CreateValidOptions() with
        {
            SecretKey = "short-key"
        };

        // Act
        var result = validator.Validate(
            name: null,
            options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(
            $"Access token secret key must have at least {AccessTokenOptions.MinimumSecretKeyLength} characters.");
    }

    [Theory(DisplayName = "Should fail when expiration minutes is invalid")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Fail_When_ExpirationMinutes_Is_Invalid(
        int expirationMinutes)
    {
        // Arrange
        var validator = new AccessTokenOptionsValidator();

        var options = CreateValidOptions() with
        {
            ExpirationMinutes = expirationMinutes
        };

        // Act
        var result = validator.Validate(
            name: null,
            options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain("Access token expiration must be greater than zero.");
    }

    [Fact(DisplayName = "Should return all failures when multiple options are invalid")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Return_All_Failures_When_Multiple_Options_Are_Invalid()
    {
        // Arrange
        var validator = new AccessTokenOptionsValidator();

        var options = new AccessTokenOptions
        {
            Issuer = string.Empty,
            Audience = string.Empty,
            SecretKey = string.Empty,
            ExpirationMinutes = 0
        };

        // Act
        var result = validator.Validate(
            name: null,
            options);

        // Assert
        result.Failed.Should().BeTrue();

        result.Failures.Should().Contain("Access token issuer was not configured.");
        result.Failures.Should().Contain("Access token audience was not configured.");
        result.Failures.Should().Contain("Access token secret key was not configured.");
        result.Failures.Should().Contain("Access token expiration must be greater than zero.");
    }

    private static AccessTokenOptions CreateValidOptions()
    {
        return new AccessTokenOptions
        {
            Issuer = "GAUSS.Identity",
            Audience = "GAUSS.Platform",
            SecretKey = "test-secret-key-with-at-least-32-characters",
            ExpirationMinutes = 15
        };
    }
}
