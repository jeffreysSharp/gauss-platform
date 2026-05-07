using Gauss.Identity.Domain.Roles;
using Gauss.Identity.Domain.Roles.ValueObjects;
using Gauss.Identity.Domain.Tenants;
using Gauss.Identity.Domain.Users;

namespace Gauss.Identity.Application.Abstractions.Persistence;

public interface IRoleRepository
{
    Task<bool> ExistsByNameAsync(
        TenantId tenantId,
        RoleName name,
        CancellationToken cancellationToken = default);

    Task<Role?> GetByIdAsync(
        RoleId roleId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Role>> GetByUserAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Role role,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Role role,
        CancellationToken cancellationToken = default);

    Task AssignToUserAsync(
        UserRole userRole,
        CancellationToken cancellationToken = default);
}
