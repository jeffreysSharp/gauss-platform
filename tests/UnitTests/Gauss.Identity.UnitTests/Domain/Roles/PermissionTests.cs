using AwesomeAssertions;
using Gauss.Identity.Domain.Roles;
using Gauss.Identity.Domain.Roles.ValueObjects;

namespace Gauss.Identity.UnitTests.Domain.Roles;

public sealed class PermissionTests
{
    [Fact(DisplayName = "Should create permission when data is valid")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Roles")]
    public void Should_Create_Permission_When_Data_Is_Valid()
    {
        // Arrange
        var createdAtUtc = new DateTimeOffset(2026, 05, 07, 12, 0, 0, TimeSpan.Zero);

        // Act
        var permission = Permission.Create(
            PermissionCode.Create("Identity.Users.Read"),
            "Read identity users.",
            createdAtUtc);

        // Assert
        permission.Id.Value.Should().NotBe(Guid.Empty);
        permission.Code.Should().Be(PermissionCode.Create("Identity.Users.Read"));
        permission.Description.Should().Be("Read identity users.");
        permission.CreatedAtUtc.Should().Be(createdAtUtc);
        permission.IsEnabled.Should().BeTrue();
    }

    [Fact(DisplayName = "Should update permission description")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Roles")]
    public void Should_Update_Permission_Description()
    {
        // Arrange
        var permission = CreatePermission();

        // Act
        permission.UpdateDescription("  Updated description.  ");

        // Assert
        permission.Description.Should().Be("Updated description.");
    }

    [Fact(DisplayName = "Should disable permission")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Roles")]
    public void Should_Disable_Permission()
    {
        // Arrange
        var permission = CreatePermission();

        // Act
        permission.Disable();

        // Assert
        permission.IsEnabled.Should().BeFalse();
    }

    [Fact(DisplayName = "Should enable permission")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Roles")]
    public void Should_Enable_Permission()
    {
        // Arrange
        var permission = CreatePermission();

        permission.Disable();

        // Act
        permission.Enable();

        // Assert
        permission.IsEnabled.Should().BeTrue();
    }

    private static Permission CreatePermission()
    {
        return Permission.Create(
            PermissionCode.Create("Identity.Users.Read"),
            "Read identity users.",
            new DateTimeOffset(2026, 05, 07, 12, 0, 0, TimeSpan.Zero));
    }
}
