namespace Gauss.Identity.Api.Installers;

public sealed class OpenApiInstaller : IInstaller
{
    public void InstallServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOpenApi("v1");
    }
}
