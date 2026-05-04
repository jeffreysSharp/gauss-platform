using Gauss.BuildingBlocks.Api.Responses;
using Gauss.BuildingBlocks.Application.Abstractions.Messaging;
using Gauss.Identity.Application.Authentication.Login;
using Microsoft.AspNetCore.Mvc;


namespace Gauss.Identity.Api.Endpoints.Authentication;

public static class LoginEndpoint
{
    public static IEndpointRouteBuilder MapLoginEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/identity/login", HandleAsync)
            .WithName("Login")
            .WithTags("Identity")
            .WithSummary("Authenticate user")
            .WithDescription("Authenticates a GAUSS Platform user and returns a short-lived access token.")
            .Accepts<LoginRequest>("application/json")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        LoginRequest request,
        [FromServices] ICommandHandler<LoginCommand, LoginResponse> handler,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand();

        var result = await handler.HandleAsync(
            command,
            cancellationToken);

        return result.ToHttpResult(Results.Ok);
    }
}
