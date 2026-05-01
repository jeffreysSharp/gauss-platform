namespace Gauss.Identity.Api.Installers;

public interface IInstaller
{
    int Order => 0;

    void InstallServices(
        IServiceCollection services,
        IConfiguration configuration);
}
