using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Infrastructure.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Gauss.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, AspNetCorePasswordHasher>();

        return services;
    }
}
