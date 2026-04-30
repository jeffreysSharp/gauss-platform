using AwesomeAssertions;
using Gauss.Identity.Domain.Users.ValueObjects;

namespace Gauss.Identity.UnitTests.Domain.Users;

public sealed class PasswordHashTests
{
    [Fact(DisplayName = "Should create password hash when value is valid")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Create_PasswordHash_When_Value_Is_Valid()
    {
        // Arrange
        const string value = "hashed-password-value";

        // Act
        var passwordHash = PasswordHash.Create(value);

        // Assert
        passwordHash.Value.Should().Be(value);
    }

    [Theory(DisplayName = "Should throw argument exception when password hash value is invalid")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_Throw_ArgumentException_When_PasswordHash_Value_Is_Invalid(string value)
    {
        // Act
        var action = () => PasswordHash.Create(value);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Should return protected text when password hash is converted to string")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Return_Protected_Text_When_PasswordHash_Is_Converted_To_String()
    {
        // Arrange
        var passwordHash = PasswordHash.Create("hashed-password-value");

        // Act
        var value = passwordHash.ToString();

        // Assert
        value.Should().Be("[PROTECTED]");
    }

    [Fact(DisplayName = "Should return true when password hashes have same value")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Return_True_When_PasswordHashes_Have_Same_Value()
    {
        // Arrange
        var first = PasswordHash.Create("hashed-password-value");
        var second = PasswordHash.Create("hashed-password-value");

        // Act & Assert
        first.Should().Be(second);
    }
}
