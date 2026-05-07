using Gauss.BuildingBlocks.Application.Abstractions.Messaging;

namespace Gauss.Identity.Application.Authorization.GetPermissions;

public sealed record GetPermissionsQuery
    : IQuery<IReadOnlyCollection<GetPermissionResponse>>;
