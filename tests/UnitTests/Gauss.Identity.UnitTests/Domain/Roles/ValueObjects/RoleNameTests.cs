using AwesomeAssertions;
using Gauss.Identity.Domain.Roles.ValueObjects;

namespace Gauss.Identity.UnitTests.Domain.Roles.ValueObjects;

public sealed class RoleNameTests
{
    [Fact(DisplayName = "Should create role name when value is valid")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Create_RoleName_When_Value_Is_Valid()
    {
        // Act
        var roleName = RoleName.Create("Admin");

        // Assert
        roleName.Value.Should().Be("Admin");
    }

    [Fact(DisplayName = "Should trim role name")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Trim_RoleName()
    {
        // Act
        var roleName = RoleName.Create("  Admin  ");

        // Assert
        roleName.Value.Should().Be("Admin");
    }

    [Fact(DisplayName = "Should throw argument exception when role name is empty")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Throw_ArgumentException_When_RoleName_Is_Empty()
    {
        // Act
        var action = () => RoleName.Create(string.Empty);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Should throw argument exception when role name exceeds maximum length")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Throw_ArgumentException_When_RoleName_Exceeds_Maximum_Length()
    {
        // Arrange
        var value = new string('A', RoleName.MaxLength + 1);

        // Act
        var action = () => RoleName.Create(value);

        // Assert
        action.Should().Throw<ArgumentException>();
    }
}
