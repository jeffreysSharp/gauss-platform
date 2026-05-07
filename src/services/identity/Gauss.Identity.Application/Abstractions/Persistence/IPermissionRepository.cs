using Gauss.Identity.Domain.Roles;
using Gauss.Identity.Domain.Roles.ValueObjects;

namespace Gauss.Identity.Application.Abstractions.Persistence;

public interface IPermissionRepository
{
    Task<bool> ExistsByCodeAsync(
        PermissionCode code,
        CancellationToken cancellationToken = default);

    Task<Permission?> GetByCodeAsync(
        PermissionCode code,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Permission>> GetAllEnabledAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Permission permission,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Permission permission,
        CancellationToken cancellationToken = default);
}
