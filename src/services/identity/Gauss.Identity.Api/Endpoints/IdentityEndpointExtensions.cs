using Gauss.Identity.Api.Endpoints.Authentication;
using Gauss.Identity.Api.Endpoints.Authorization;
using Gauss.Identity.Api.Endpoints.Users;

namespace Gauss.Identity.Api.Endpoints;

public static class IdentityEndpointExtensions
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapRegisterUserEndpoint();
        app.MapLoginEndpoint();
        app.MapCurrentUserEndpoint();
        app.MapRefreshTokenEndpoint();
        app.MapGetPermissionsEndpoint();

        return app;
    }
}
