using AwesomeAssertions;
using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Infrastructure.Authentication;

namespace Gauss.Identity.InfrastructureTests.Authentication;

public sealed class AspNetCorePasswordHasherTests
{
    [Fact(DisplayName = "Should create password hash when password is valid")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Create_PasswordHash_When_Password_Is_Valid()
    {
        // Arrange
        var passwordHasher = new AspNetCorePasswordHasher();
        const string password = "StrongPassword@123";

        // Act
        var passwordHash = passwordHasher.Hash(password);

        // Assert
        passwordHash.Value.Should().NotBeNullOrWhiteSpace();
        passwordHash.Value.Should().NotBe(password);
        passwordHash.ToString().Should().Be("[PROTECTED]");
    }

    [Fact(DisplayName = "Should create different password hashes for same password")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Create_Different_PasswordHashes_For_Same_Password()
    {
        // Arrange
        var passwordHasher = new AspNetCorePasswordHasher();
        const string password = "StrongPassword@123";

        // Act
        var firstPasswordHash = passwordHasher.Hash(password);
        var secondPasswordHash = passwordHasher.Hash(password);

        // Assert
        firstPasswordHash.Value.Should().NotBe(secondPasswordHash.Value);
    }

    [Fact(DisplayName = "Should verify password when password is valid")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Verify_Password_When_Password_Is_Valid()
    {
        // Arrange
        var passwordHasher = new AspNetCorePasswordHasher();
        const string password = "StrongPassword@123";
        var passwordHash = passwordHasher.Hash(password);

        // Act
        var status = passwordHasher.Verify(passwordHash, password);

        // Assert
        status.Should().BeOneOf(
            PasswordVerificationStatus.Success,
            PasswordVerificationStatus.SuccessRehashNeeded);
    }

    [Fact(DisplayName = "Should fail password verification when password is invalid")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Fail_Password_Verification_When_Password_Is_Invalid()
    {
        // Arrange
        var passwordHasher = new AspNetCorePasswordHasher();
        var passwordHash = passwordHasher.Hash("StrongPassword@123");

        // Act
        var status = passwordHasher.Verify(passwordHash, "WrongPassword@123");

        // Assert
        status.Should().Be(PasswordVerificationStatus.Failed);
    }

    [Theory(DisplayName = "Should throw argument exception when password is invalid")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_Throw_ArgumentException_When_Password_Is_Invalid(string password)
    {
        // Arrange
        var passwordHasher = new AspNetCorePasswordHasher();

        // Act
        var action = () => passwordHasher.Hash(password);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Theory(DisplayName = "Should throw argument exception when provided password is invalid")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_Throw_ArgumentException_When_ProvidedPassword_Is_Invalid(string providedPassword)
    {
        // Arrange
        var passwordHasher = new AspNetCorePasswordHasher();
        var passwordHash = passwordHasher.Hash("StrongPassword@123");

        // Act
        var action = () => passwordHasher.Verify(passwordHash, providedPassword);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Should throw argument null exception when password hash is null")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Throw_ArgumentNullException_When_PasswordHash_Is_Null()
    {
        // Arrange
        var passwordHasher = new AspNetCorePasswordHasher();

        // Act
        var action = () => passwordHasher.Verify(
            passwordHash: null!,
            providedPassword: "StrongPassword@123");

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }
}
