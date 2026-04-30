using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Gauss.Identity.Api.HealthChecks;

public static class HealthCheckEndpointExtensions
{
    public static WebApplication MapGaussHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        })
        .WithName("LivenessHealthCheck")
        .WithTags("HealthChecks");

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("ready")
        })
        .WithName("ReadinessHealthCheck")
        .WithTags("HealthChecks");

        return app;
    }
}
