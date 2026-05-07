namespace Gauss.Identity.Application.Authorization;

public static class IdentityPermissions
{
    public const string UsersRead = "Identity.Users.Read";
    public const string UsersManage = "Identity.Users.Manage";

    public const string RolesRead = "Identity.Roles.Read";
    public const string RolesManage = "Identity.Roles.Manage";

    public const string PermissionsRead = "Identity.Permissions.Read";

    public const string TenantRead = "Identity.Tenant.Read";
    public const string TenantManage = "Identity.Tenant.Manage";
}
