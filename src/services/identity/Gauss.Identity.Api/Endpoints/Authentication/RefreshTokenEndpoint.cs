using Gauss.BuildingBlocks.Api.Responses;
using Gauss.BuildingBlocks.Application.Abstractions.Messaging;
using Gauss.Identity.Application.Authentication.RefreshTokens;
using Microsoft.AspNetCore.Mvc;

namespace Gauss.Identity.Api.Endpoints.Authentication;

public static class RefreshTokenEndpoint
{
    public static IEndpointRouteBuilder MapRefreshTokenEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/identity/refresh-token", HandleAsync)
            .WithName("RefreshToken")
            .WithTags("Identity")
            .WithSummary("Refresh access token")
            .WithDescription("Issues a new access token and refresh token using a valid refresh token.")
            .Accepts<RefreshTokenRequest>("application/json")
            .Produces<RefreshTokenResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        RefreshTokenRequest request,
        ICommandHandler<RefreshTokenCommand, RefreshTokenResponse> handler,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand();

        var result = await handler.HandleAsync(
            command,
            cancellationToken);

        return result.ToHttpResult(Results.Ok);
    }
}
