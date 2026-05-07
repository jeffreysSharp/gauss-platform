using Gauss.BuildingBlocks.Application.Abstractions.Messaging;
using Gauss.BuildingBlocks.Application.Abstractions.Results;
using Gauss.Identity.Application.Abstractions.Persistence;

namespace Gauss.Identity.Application.Authorization.GetPermissions;

public sealed class GetPermissionsQueryHandler(
    IPermissionRepository permissionRepository)
    : IQueryHandler<GetPermissionsQuery, IReadOnlyCollection<GetPermissionResponse>>
{
    public async Task<Result<IReadOnlyCollection<GetPermissionResponse>>> HandleAsync(
        GetPermissionsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var permissions = await permissionRepository.GetAllEnabledAsync(
            cancellationToken);

        var response = permissions
            .OrderBy(permission => permission.Code.Value)
            .Select(permission => new GetPermissionResponse(
                permission.Code.Value,
                permission.Description))
            .ToList();

        return Result<IReadOnlyCollection<GetPermissionResponse>>.Success(response);
    }
}
