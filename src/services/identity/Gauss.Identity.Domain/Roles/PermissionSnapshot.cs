using Gauss.Identity.Domain.Roles.ValueObjects;

namespace Gauss.Identity.Domain.Roles;

public sealed record PermissionSnapshot(
    PermissionId Id,
    PermissionCode Code,
    string Description,
    bool IsEnabled,
    DateTimeOffset CreatedAtUtc);
