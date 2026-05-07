using AwesomeAssertions;
using Gauss.Identity.Domain.Roles;
using Gauss.Identity.Domain.Tenants;
using Gauss.Identity.Domain.Users;

namespace Gauss.Identity.UnitTests.Domain.Roles;

public sealed class UserRoleTests
{
    [Fact(DisplayName = "Should assign role to user in tenant")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Roles")]
    public void Should_Assign_Role_To_User_In_Tenant()
    {
        // Arrange
        var userId = UserId.New();
        var tenantId = TenantId.New();
        var roleId = RoleId.New();
        var assignedAtUtc = new DateTimeOffset(2026, 05, 07, 12, 0, 0, TimeSpan.Zero);

        // Act
        var userRole = UserRole.Assign(
            userId,
            tenantId,
            roleId,
            assignedAtUtc);

        // Assert
        userRole.UserId.Should().Be(userId);
        userRole.TenantId.Should().Be(tenantId);
        userRole.RoleId.Should().Be(roleId);
        userRole.AssignedAtUtc.Should().Be(assignedAtUtc);
    }
}
