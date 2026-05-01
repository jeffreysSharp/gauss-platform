using Gauss.Identity.Api.Endpoints.Users;

namespace Gauss.Identity.Api.Endpoints;

public static class IdentityEndpointExtensions
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapRegisterUserEndpoint();

        return app;
    }
}
