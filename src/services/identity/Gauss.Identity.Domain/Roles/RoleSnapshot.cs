using Gauss.BuildingBlocks.Domain.Tenants;
using Gauss.Identity.Domain.Roles.ValueObjects;

namespace Gauss.Identity.Domain.Roles;

public sealed record RoleSnapshot(
    RoleId Id,
    TenantId TenantId,
    RoleName Name,
    RoleStatus Status,
    DateTimeOffset CreatedAtUtc);
