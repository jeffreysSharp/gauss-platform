using Gauss.BuildingBlocks.Api.Responses;
using Gauss.BuildingBlocks.Application.Abstractions.Messaging;
using Gauss.Identity.Api.Authorization;
using Gauss.Identity.Application.Authorization;
using Gauss.Identity.Application.Authorization.GetPermissions;
using Microsoft.AspNetCore.Mvc;

namespace Gauss.Identity.Api.Endpoints.Authorization;

public static class GetPermissionsEndpoint
{
    public static IEndpointRouteBuilder MapGetPermissionsEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/identity/permissions", HandleAsync)
            .WithName("GetPermissions")
            .WithTags("Identity")
            .WithSummary("Get identity permissions")
            .WithDescription("Returns the enabled Identity permission catalog.")
            .Produces<IReadOnlyCollection<GetPermissionResponse>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .RequireAuthorization()
            .RequirePermission(IdentityPermissions.PermissionsRead);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        IQueryHandler<GetPermissionsQuery, IReadOnlyCollection<GetPermissionResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetPermissionsQuery();

        var result = await handler.HandleAsync(
            query,
            cancellationToken);

        return result.ToHttpResult(Results.Ok);
    }
}
