using Gauss.BuildingBlocks.Api.Responses;
using Gauss.BuildingBlocks.Application.Abstractions.Results;
using Gauss.Identity.Application.Abstractions.Authorization;

namespace Gauss.Identity.Api.Authorization;

public sealed class PermissionEndpointFilter(
    string permissionCode)
    : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var permissionAuthorizationService = context.HttpContext.RequestServices
            .GetRequiredService<IPermissionAuthorizationService>();

        var hasPermission = await permissionAuthorizationService.HasPermissionAsync(
            permissionCode,
            context.HttpContext.RequestAborted);

        if (!hasPermission)
        {
            return Error.Forbidden(
                    "Identity.Permission.Denied",
                    "You do not have the required permission.")
                .ToProblemResult();
        }

        return await next(context);
    }
}
