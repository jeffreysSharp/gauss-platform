using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Application.Abstractions.Time;
using Gauss.Identity.Infrastructure.Authentication;
using Gauss.Identity.Infrastructure.Persistence;
using Gauss.Identity.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gauss.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<IdentityPersistenceOptions>(
            configuration.GetSection(IdentityPersistenceOptions.SectionName));

        services.AddSingleton<IdentityDbConnectionFactory>();

        services.AddScoped<IUserRepository, SqlUserRepository>();

        services.AddSingleton<IPasswordHasher, AspNetCorePasswordHasher>();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.Configure<AccessTokenOptions>(configuration.GetSection(AccessTokenOptions.SectionName));

        services.AddSingleton<IAccessTokenProvider, JwtAccessTokenProvider>();

        return services;
    }
}
