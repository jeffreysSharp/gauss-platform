using Gauss.Identity.Api.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Gauss.Identity.Api.Installers;

public sealed class AuthenticationInstaller : IInstaller
{
    public void InstallServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddAuthorization();

        services.AddSingleton<
            IConfigureOptions<JwtBearerOptions>,
            JwtBearerOptionsSetup>();
    }
}
