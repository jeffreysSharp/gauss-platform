using Gauss.Identity.Domain.Roles.ValueObjects;

namespace Gauss.Identity.Application.Abstractions.Authorization;

public interface IPermissionAuthorizationService
{
    Task<bool> HasPermissionAsync(
        PermissionCode permissionCode,
        CancellationToken cancellationToken = default);
}
