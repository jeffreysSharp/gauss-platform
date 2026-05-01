using Scalar.AspNetCore;

namespace Gauss.Identity.Api.OpenApi;

public static class OpenApiEndpointExtensions
{
    public static WebApplication MapGaussOpenApi(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.MapOpenApi();

        app.MapScalarApiReference("/docs", options =>
        {
            options.Title = "GAUSS Identity API";
            options.Theme = ScalarTheme.DeepSpace;
            options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

        return app;
    }
}
