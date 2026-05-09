using Gauss.BuildingBlocks.Domain.Tenants;
using Gauss.Identity.Domain.Users.ValueObjects;

namespace Gauss.Identity.Domain.Users;

public sealed record UserSnapshot(
    UserId Id,
    TenantId TenantId,
    string Name,
    Email Email,
    PasswordHash PasswordHash,
    UserStatus Status,
    DateTimeOffset RegisteredAtUtc,
    DateTimeOffset? EmailConfirmedAtUtc,
    DateTimeOffset? LastLoginAtUtc,
    DateTimeOffset? LockedUntilUtc);
