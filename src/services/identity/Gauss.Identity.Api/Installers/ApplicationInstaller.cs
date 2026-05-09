using Gauss.Identity.Application.DependencyInjection;

namespace Gauss.Identity.Api.Installers;

public sealed class ApplicationInstaller : IInstaller
{
    public int Order => InstallerOrder.Application;

    public void InstallServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentityApplication();
    }
}
