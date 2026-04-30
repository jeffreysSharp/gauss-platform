using AwesomeAssertions;
using Gauss.Identity.Domain.Users.ValueObjects;

namespace Gauss.Identity.UnitTests.Domain.Users;

public sealed class EmailTests
{
    [Fact(DisplayName = "Should create email when value is valid")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Create_Email_When_Value_Is_Valid()
    {
        // Arrange
        const string value = "user@gauss.com";

        // Act
        var email = Email.Create(value);

        // Assert
        email.Value.Should().Be(value);
    }

    [Fact(DisplayName = "Should normalize email when value has uppercase and spaces")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Normalize_Email_When_Value_Has_Uppercase_And_Spaces()
    {
        // Arrange
        const string value = "  USER@GAUSS.COM  ";

        // Act
        var email = Email.Create(value);

        // Assert
        email.Value.Should().Be("user@gauss.com");
    }

    [Theory(DisplayName = "Should throw argument exception when email value is invalid")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    [InlineData("@gauss.com")]
    public void Should_Throw_ArgumentException_When_Email_Value_Is_Invalid(string value)
    {
        // Act
        var action = () => Email.Create(value);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Should return true when emails have same normalized value")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Return_True_When_Emails_Have_Same_Normalized_Value()
    {
        // Arrange
        var first = Email.Create("USER@GAUSS.COM");
        var second = Email.Create("user@gauss.com");

        // Act & Assert
        first.Should().Be(second);
    }
}
