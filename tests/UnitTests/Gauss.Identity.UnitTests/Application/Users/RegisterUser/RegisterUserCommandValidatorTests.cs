using AwesomeAssertions;
using FluentValidation.TestHelper;
using Gauss.Identity.Application.Users.RegisterUser;

namespace Gauss.Identity.UnitTests.Application.Users.RegisterUser;

public sealed class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact(DisplayName = "Should not have validation errors when command is valid")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Validators")]
    public void Should_Not_Have_Validation_Errors_When_Command_Is_Valid()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "Jeferson Almeida",
            "jeferson@gauss.com",
            "StrongPassword@123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory(DisplayName = "Should have validation error when name is invalid")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Validators")]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_Have_Validation_Error_When_Name_Is_Invalid(string name)
    {
        // Arrange
        var command = new RegisterUserCommand(
            name,
            "jeferson@gauss.com",
            "StrongPassword@123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(command => command.Name);
    }

    [Fact(DisplayName = "Should have validation error when name is too long")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Validators")]
    public void Should_Have_Validation_Error_When_Name_Is_Too_Long()
    {
        // Arrange
        var command = new RegisterUserCommand(
            new string('A', 151),
            "jeferson@gauss.com",
            "StrongPassword@123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(command => command.Name);
    }

    [Theory(DisplayName = "Should have validation error when email is invalid")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Validators")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    [InlineData("@gauss.com")]
    public void Should_Have_Validation_Error_When_Email_Is_Invalid(string email)
    {
        // Arrange
        var command = new RegisterUserCommand(
            "Jeferson Almeida",
            email,
            "StrongPassword@123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(command => command.Email);
    }

    [Fact(DisplayName = "Should have validation error when email is too long")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Validators")]
    public void Should_Have_Validation_Error_When_Email_Is_Too_Long()
    {
        // Arrange
        var email = $"{new string('a', 245)}@gauss.com";

        var command = new RegisterUserCommand(
            "Jeferson Almeida",
            email,
            "StrongPassword@123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(command => command.Email);
    }

    [Theory(DisplayName = "Should have validation error when password is invalid")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Validators")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("short")]
    [InlineData("lowercase@123")]
    [InlineData("UPPERCASE@123")]
    [InlineData("NoDigits@")]
    [InlineData("NoSpecial123")]
    public void Should_Have_Validation_Error_When_Password_Is_Invalid(string password)
    {
        // Arrange
        var command = new RegisterUserCommand(
            "Jeferson Almeida",
            "jeferson@gauss.com",
            password);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(command => command.Password);
    }
}
