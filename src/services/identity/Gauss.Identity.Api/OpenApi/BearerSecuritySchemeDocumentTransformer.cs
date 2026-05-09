using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Gauss.Identity.Api.OpenApi;

public sealed class BearerSecuritySchemeDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes ??=
            new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);

        document.Components.SecuritySchemes[BearerSecurityScheme.Name] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Bearer authentication. Use the format: Bearer {token}"
        };

        return Task.CompletedTask;
    }
}
