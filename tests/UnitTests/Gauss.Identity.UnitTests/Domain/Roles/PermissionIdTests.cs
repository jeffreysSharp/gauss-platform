using AwesomeAssertions;
using Gauss.Identity.Domain.Roles;

namespace Gauss.Identity.UnitTests.Domain.Roles;

public sealed class PermissionIdTests
{
    [Fact(DisplayName = "Should create new permission id")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Create_New_PermissionId()
    {
        // Act
        var permissionId = PermissionId.New();

        // Assert
        permissionId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "Should create permission id from guid")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Create_PermissionId_From_Guid()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var permissionId = PermissionId.From(value);

        // Assert
        permissionId.Value.Should().Be(value);
    }

    [Fact(DisplayName = "Should throw argument exception when permission id is empty")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "ValueObjects")]
    public void Should_Throw_ArgumentException_When_PermissionId_Is_Empty()
    {
        // Act
        var action = () => PermissionId.From(Guid.Empty);

        // Assert
        action.Should().Throw<ArgumentException>();
    }
}
