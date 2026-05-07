using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Abstractions.Tenancy;
using Microsoft.AspNetCore.Mvc;

namespace Gauss.Identity.Api.Endpoints.Authentication;

public static class CurrentUserEndpoint
{
    public static IEndpointRouteBuilder MapCurrentUserEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/identity/me", HandleAsync)
            .WithName("GetCurrentUser")
            .WithTags("Identity")
            .WithSummary("Get current user")
            .WithDescription("Returns the authenticated GAUSS Platform user identity and tenant context.")
            .Produces<CurrentUserResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return app;
    }

    private static IResult HandleAsync(
        [FromServices] ICurrentUserContext currentUserContext,
        [FromServices] ICurrentTenantContext currentTenantContext)
    {
        if (!currentUserContext.IsAuthenticated ||
            currentUserContext.UserId is null ||
            !currentTenantContext.HasTenant ||
            currentTenantContext.CurrentTenantId is null ||
            string.IsNullOrWhiteSpace(currentUserContext.Name) ||
            string.IsNullOrWhiteSpace(currentUserContext.Email))
        {
            return Results.Unauthorized();
        }

        var response = new CurrentUserResponse(
            currentUserContext.UserId.Value,
            currentTenantContext.CurrentTenantId.Value.Value,
            currentUserContext.Name,
            currentUserContext.Email);

        return Results.Ok(response);
    }
}
