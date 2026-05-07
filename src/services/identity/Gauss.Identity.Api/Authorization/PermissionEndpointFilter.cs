using Gauss.Identity.Application.Abstractions.Authorization;
using Gauss.Identity.Domain.Roles.ValueObjects;

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
            PermissionCode.Create(permissionCode),
            context.HttpContext.RequestAborted);

        if (!hasPermission)
        {
            return Results.Forbid();
        }

        return await next(context);
    }
}
