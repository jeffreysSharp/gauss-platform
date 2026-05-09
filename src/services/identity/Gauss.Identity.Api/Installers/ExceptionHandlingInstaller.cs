using Gauss.Identity.Api.Observability;

namespace Gauss.Identity.Api.Installers;

public sealed class ExceptionHandlingInstaller : IInstaller
{
    public int Order => 50;

    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        
    }

    public static void UseExceptionHandling(IApplicationBuilder app)
    {
        app.UseGaussExceptionHandling();
    }
}
