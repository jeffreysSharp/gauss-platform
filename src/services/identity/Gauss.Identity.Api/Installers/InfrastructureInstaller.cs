using Gauss.Identity.Infrastructure;

namespace Gauss.Identity.Api.Installers;

public sealed class InfrastructureInstaller : IInstaller
{
    public void InstallServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentityInfrastructure(configuration);
    }
}
