using AwesomeAssertions;
using Gauss.BuildingBlocks.Domain.Tenants;
using Gauss.Identity.Domain.Roles;
using Gauss.Identity.Domain.Roles.ValueObjects;

namespace Gauss.Identity.UnitTests.Domain.Roles;

public sealed class RoleTests
{
    [Fact(DisplayName = "Should create role when data is valid")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Roles")]
    public void Should_Create_Role_When_Data_Is_Valid()
    {
        // Arrange
        var tenantId = TenantId.New();
        var createdAtUtc = new DateTimeOffset(2026, 05, 07, 12, 0, 0, TimeSpan.Zero);

        // Act
        var role = Role.Create(
            tenantId,
            RoleName.Create("Admin"),
            createdAtUtc);

        // Assert
        role.Id.Value.Should().NotBe(Guid.Empty);
        role.TenantId.Should().Be(tenantId);
        role.Name.Should().Be(RoleName.Create("Admin"));
        role.Status.Should().Be(RoleStatus.Active);
        role.IsActive.Should().BeTrue();
        role.CreatedAtUtc.Should().Be(createdAtUtc);
        role.Permissions.Should().BeEmpty();
    }

    [Fact(DisplayName = "Should rename role")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Roles")]
    public void Should_Rename_Role()
    {
        // Arrange
        var role = CreateRole();

        // Act
        role.Rename(RoleName.Create("Manager"));

        // Assert
        role.Name.Should().Be(RoleName.Create("Manager"));
    }

    [Fact(DisplayName = "Should grant permission to role")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Roles")]
    public void Should_Grant_Permission_To_Role()
    {
        // Arrange
        var role = CreateRole();

        var permission = CreatePermission("Identity.Users.Read");

        // Act
        role.GrantPermission(permission);

        // Assert
        role.Permissions.Should().ContainSingle();

        var rolePermission = role.Permissions.Single();

        rolePermission.RoleId.Should().Be(role.Id);
        rolePermission.PermissionId.Should().Be(permission.Id);
        rolePermission.PermissionCode.Should().Be(permission.Code);

        role.HasPermission(permission.Code).Should().BeTrue();
    }

    [Fact(DisplayName = "Should not duplicate permission when permission already exists")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Roles")]
    public void Should_Not_Duplicate_Permission_When_Permission_Already_Exists()
    {
        // Arrange
        var role = CreateRole();

        var permission = CreatePermission("Identity.Users.Read");

        role.GrantPermission(permission);

        // Act
        role.GrantPermission(permission);

        // Assert
        role.Permissions.Should().ContainSingle();
    }

    [Fact(DisplayName = "Should revoke permission from role")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Roles")]
    public void Should_Revoke_Permission_From_Role()
    {
        // Arrange
        var role = CreateRole();

        var permission = CreatePermission("Identity.Users.Read");

        role.GrantPermission(permission);

        // Act
        role.RevokePermission(permission.Code);

        // Assert
        role.Permissions.Should().BeEmpty();
        role.HasPermission(permission.Code).Should().BeFalse();
    }

    [Fact(DisplayName = "Should throw invalid operation exception when granting disabled permission")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Roles")]
    public void Should_Throw_InvalidOperationException_When_Granting_Disabled_Permission()
    {
        // Arrange
        var role = CreateRole();

        var permission = CreatePermission("Identity.Users.Read");

        permission.Disable();

        // Act
        var action = () => role.GrantPermission(permission);

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact(DisplayName = "Should deactivate role")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Roles")]
    public void Should_Deactivate_Role()
    {
        // Arrange
        var role = CreateRole();

        // Act
        role.Deactivate();

        // Assert
        role.Status.Should().Be(RoleStatus.Inactive);
        role.IsActive.Should().BeFalse();
    }

    [Fact(DisplayName = "Should activate role")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Roles")]
    public void Should_Activate_Role()
    {
        // Arrange
        var role = CreateRole();

        role.Deactivate();

        // Act
        role.Activate();

        // Assert
        role.Status.Should().Be(RoleStatus.Active);
        role.IsActive.Should().BeTrue();
    }

    private static Role CreateRole()
    {
        return Role.Create(
            TenantId.New(),
            RoleName.Create("Admin"),
            new DateTimeOffset(2026, 05, 07, 12, 0, 0, TimeSpan.Zero));
    }

    private static Permission CreatePermission(string code)
    {
        return Permission.Create(
            PermissionCode.Create(code),
            $"Permission {code}.",
            new DateTimeOffset(2026, 05, 07, 12, 0, 0, TimeSpan.Zero));
    }
}
