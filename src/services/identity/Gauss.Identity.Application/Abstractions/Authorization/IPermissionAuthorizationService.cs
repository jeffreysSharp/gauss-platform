namespace Gauss.Identity.Application.Abstractions.Authorization;

public interface IPermissionAuthorizationService
{
    Task<bool> HasPermissionAsync(
        string permissionCode,
        CancellationToken cancellationToken = default);
}
