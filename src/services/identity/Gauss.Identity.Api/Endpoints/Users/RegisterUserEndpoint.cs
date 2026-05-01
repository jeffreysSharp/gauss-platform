using Gauss.BuildingBlocks.Api.Responses;
using Gauss.BuildingBlocks.Application.Abstractions.Messaging;
using Gauss.Identity.Application.Users.RegisterUser;
using Microsoft.AspNetCore.Mvc;

namespace Gauss.Identity.Api.Endpoints.Users;

public static class RegisterUserEndpoint
{
    public static IEndpointRouteBuilder MapRegisterUserEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/identity/register", HandleAsync)
            .WithName("RegisterUser")
            .WithTags("Identity")
            .WithSummary("Register a new user")
            .WithDescription("Registers a new GAUSS Platform user and creates the initial tenant context.")
            .Accepts<RegisterUserRequest>("application/json")
            .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        RegisterUserRequest request,
        ICommandHandler<RegisterUserCommand, RegisterUserResponse> handler,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand();

        var result = await handler.HandleAsync(
            command,
            cancellationToken);

        return result.ToHttpResult(response =>
            Results.Created(
                $"/api/v1/identity/users/{response.UserId}",
                response));
    }
}
