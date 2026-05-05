using Gauss.Identity.Application.Abstractions.Authentication;
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
        [FromServices] ICurrentUserContext currentUserContext)
    {
        if (!currentUserContext.IsAuthenticated ||
            currentUserContext.UserId is null ||
            currentUserContext.TenantId is null ||
            string.IsNullOrWhiteSpace(currentUserContext.Name) ||
            string.IsNullOrWhiteSpace(currentUserContext.Email))
        {
            return Results.Unauthorized();
        }

        var response = new CurrentUserResponse(
            currentUserContext.UserId.Value,
            currentUserContext.TenantId.Value,
            currentUserContext.Name,
            currentUserContext.Email);

        return Results.Ok(response);
    }
}
