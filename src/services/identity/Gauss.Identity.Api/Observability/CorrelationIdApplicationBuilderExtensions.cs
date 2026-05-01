namespace Gauss.Identity.Api.Observability;

public static class CorrelationIdApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGaussCorrelationId(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
