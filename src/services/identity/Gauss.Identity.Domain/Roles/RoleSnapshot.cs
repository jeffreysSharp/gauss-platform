using Gauss.Identity.Domain.Roles.ValueObjects;
using Gauss.Identity.Domain.Tenants;

namespace Gauss.Identity.Domain.Roles;

public sealed record RoleSnapshot(
    RoleId Id,
    TenantId TenantId,
    RoleName Name,
    RoleStatus Status,
    DateTimeOffset CreatedAtUtc);
