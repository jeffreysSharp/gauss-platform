using Gauss.BuildingBlocks.Domain.Entities;
using Gauss.Identity.Domain.Roles.ValueObjects;
using Gauss.Identity.Domain.Tenants;
using Gauss.Identity.Domain.Users;

namespace Gauss.Identity.Domain.Roles;

public sealed record UserRole(
    UserId UserId,
    TenantId TenantId,
    RoleId RoleId,
    DateTimeOffset AssignedAtUtc)
{
    public static UserRole Assign(
        UserId userId,
        TenantId tenantId,
        RoleId roleId,
        DateTimeOffset assignedAtUtc)
    {
        return new UserRole(
            userId,
            tenantId,
            roleId,
            assignedAtUtc);
    }
}
