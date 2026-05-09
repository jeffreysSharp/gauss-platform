using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Abstractions.Authorization;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Application.Abstractions.Tenancy;
using Gauss.Identity.Domain.Roles.ValueObjects;
using Gauss.Identity.Domain.Users;

namespace Gauss.Identity.Application.Authorization;

public sealed class PermissionAuthorizationService(
    ICurrentUserContext currentUserContext,
    ICurrentTenantContext currentTenantContext,
    IRoleRepository roleRepository)
    : IPermissionAuthorizationService
{
    public async Task<bool> HasPermissionAsync(
        string permissionCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
        {
            return false;
        }

        if (!currentUserContext.IsAuthenticated ||
            currentUserContext.UserId is null ||
            !currentTenantContext.HasTenant ||
            currentTenantContext.CurrentTenantId is null)
        {
            return false;
        }

        var code = PermissionCode.Create(permissionCode);

        var userId = UserId.From(currentUserContext.UserId.Value);

        var roles = await roleRepository.GetByUserAsync(
            currentTenantContext.CurrentTenantId.Value,
            userId,
            cancellationToken);

        return roles.Any(role =>
            role.IsActive &&
            role.HasPermission(code));
    }
}
