namespace Gauss.Identity.Api.Authorization;

public static class PermissionEndpointConventionBuilderExtensions
{
    public static TBuilder RequirePermission<TBuilder>(
        this TBuilder builder,
        string permissionCode)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permissionCode);

        builder.AddEndpointFilter(new PermissionEndpointFilter(permissionCode));

        return builder;
    }
}
