using Gauss.Identity.Domain.Roles.ValueObjects;

namespace Gauss.Identity.Domain.Roles;

public sealed record RolePermission(
    RoleId RoleId,
    PermissionId PermissionId,
    PermissionCode PermissionCode)
{
    public static RolePermission Create(
        RoleId roleId,
        Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        return new RolePermission(
            roleId,
            permission.Id,
            permission.Code);
    }
}
