using AwesomeAssertions;
using Gauss.Identity.Domain.Roles.ValueObjects;

namespace Gauss.Identity.UnitTests.Domain.Roles.ValueObjects;

public sealed class PermissionCodeTests
{
    [Fact(DisplayName = "Should create permission code when value is valid")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Create_PermissionCode_When_Value_Is_Valid()
    {
        // Act
        var permissionCode = PermissionCode.Create("Identity.Users.Read");

        // Assert
        permissionCode.Value.Should().Be("Identity.Users.Read");
    }

    [Fact(DisplayName = "Should trim permission code")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Trim_PermissionCode()
    {
        // Act
        var permissionCode = PermissionCode.Create("  Identity.Users.Read  ");

        // Assert
        permissionCode.Value.Should().Be("Identity.Users.Read");
    }

    [Fact(DisplayName = "Should throw argument exception when permission code is empty")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Throw_ArgumentException_When_PermissionCode_Is_Empty()
    {
        // Act
        var action = () => PermissionCode.Create(string.Empty);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Should throw argument exception when permission code exceeds maximum length")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Throw_ArgumentException_When_PermissionCode_Exceeds_Maximum_Length()
    {
        // Arrange
        var value = new string('A', PermissionCode.MaxLength + 1);

        // Act
        var action = () => PermissionCode.Create(value);

        // Assert
        action.Should().Throw<ArgumentException>();
    }
}
