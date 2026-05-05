using Gauss.Identity.Api.Authentication;
using Gauss.Identity.Application.Abstractions.Authentication;
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

        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();

        services.AddSingleton<
            IConfigureOptions<JwtBearerOptions>,
            JwtBearerOptionsSetup>();
    }
}
