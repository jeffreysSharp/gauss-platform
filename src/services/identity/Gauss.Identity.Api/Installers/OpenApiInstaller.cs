namespace Gauss.Identity.Api.Installers;

public sealed class OpenApiInstaller : IInstaller
{
    public int Order => InstallerOrder.OpenApi;

    public void InstallServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOpenApi("v1");
    }
}
