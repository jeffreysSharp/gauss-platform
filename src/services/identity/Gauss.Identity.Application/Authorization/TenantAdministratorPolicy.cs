namespace Gauss.Identity.Application.Authorization;

public static class TenantAdministratorPolicy
{
    public static IReadOnlyCollection<string> BaselinePermissions { get; } =
    [
        IdentityPermissions.UsersRead,
        IdentityPermissions.UsersManage,
        IdentityPermissions.RolesRead,
        IdentityPermissions.RolesManage,
        IdentityPermissions.PermissionsRead,
        IdentityPermissions.TenantRead,
        IdentityPermissions.TenantManage
    ];
}
