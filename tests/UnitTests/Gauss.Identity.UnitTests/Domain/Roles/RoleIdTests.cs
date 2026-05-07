using AwesomeAssertions;
using Gauss.Identity.Domain.Roles;

namespace Gauss.Identity.UnitTests.Domain.Roles;

public sealed class RoleIdTests
{
    [Fact(DisplayName = "Should create new role id")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Create_New_RoleId()
    {
        // Act
        var roleId = RoleId.New();

        // Assert
        roleId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "Should create role id from guid")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Create_RoleId_From_Guid()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var roleId = RoleId.From(value);

        // Assert
        roleId.Value.Should().Be(value);
    }

    [Fact(DisplayName = "Should throw argument exception when role id is empty")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Throw_ArgumentException_When_RoleId_Is_Empty()
    {
        // Act
        var action = () => RoleId.From(Guid.Empty);

        // Assert
        action.Should().Throw<ArgumentException>();
    }
}
