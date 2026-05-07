using Gauss.BuildingBlocks.Domain.Entities;
using Gauss.Identity.Domain.Roles.ValueObjects;
using Gauss.Identity.Domain.Tenants;

namespace Gauss.Identity.Domain.Roles;

public sealed class Role : AggregateRoot<RoleId>
{
    private readonly List<RolePermission> _permissions = [];

    private Role(
        RoleId id,
        TenantId tenantId,
        RoleName name,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Status = RoleStatus.Active;
        CreatedAtUtc = createdAtUtc;
    }

    public TenantId TenantId { get; private init; }

    public RoleName Name { get; private set; }

    public RoleStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private init; }

    public IReadOnlyCollection<RolePermission> Permissions =>
        _permissions.AsReadOnly();

    public bool IsActive => Status == RoleStatus.Active;

    public static Role Create(
        TenantId tenantId,
        RoleName name,
        DateTimeOffset createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(name);

        return new Role(
            RoleId.New(),
            tenantId,
            name,
            createdAtUtc);
    }

    public void Rename(RoleName name)
    {
        ArgumentNullException.ThrowIfNull(name);

        Name = name;
    }

    public void GrantPermission(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        if (!permission.IsEnabled)
        {
            throw new InvalidOperationException(
                "Cannot grant a disabled permission to a role.");
        }

        if (HasPermission(permission.Code))
        {
            return;
        }

        _permissions.Add(RolePermission.Create(
            Id,
            permission));
    }

    public void RevokePermission(PermissionCode permissionCode)
    {
        ArgumentNullException.ThrowIfNull(permissionCode);

        _permissions.RemoveAll(rolePermission =>
            rolePermission.PermissionCode == permissionCode);
    }

    public bool HasPermission(PermissionCode permissionCode)
    {
        ArgumentNullException.ThrowIfNull(permissionCode);

        return _permissions.Exists(rolePermission =>
            rolePermission.PermissionCode == permissionCode);
    }

    public void Activate()
    {
        Status = RoleStatus.Active;
    }

    public void Deactivate()
    {
        Status = RoleStatus.Inactive;
    }
}
