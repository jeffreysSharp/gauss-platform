namespace Gauss.Identity.Api.Installers;

public interface IInstaller
{
    int Order { get; }

    void InstallServices(
        IServiceCollection services,
        IConfiguration configuration);
}
