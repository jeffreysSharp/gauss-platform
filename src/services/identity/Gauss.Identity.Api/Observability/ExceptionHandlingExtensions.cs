using Gauss.Identity.Api.Middleware;

namespace Gauss.Identity.Api.Observability;

public static class ExceptionHandlingExtensions
{
    public static IApplicationBuilder UseGaussExceptionHandling(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
