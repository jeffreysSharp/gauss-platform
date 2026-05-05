using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Application.Abstractions.Time;
using Gauss.Identity.Infrastructure.Authentication;
using Gauss.Identity.Infrastructure.Persistence;
using Gauss.Identity.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Gauss.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .Configure<IdentityPersistenceOptions>(
            configuration.GetSection(IdentityPersistenceOptions.SectionName));

        services.AddSingleton<IdentityDbConnectionFactory>();

        services.AddScoped<IUserRepository, SqlUserRepository>();

        services.AddSingleton<IPasswordHasher, AspNetCorePasswordHasher>();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services
            .AddOptions<AccessTokenOptions>()
            .Bind(configuration.GetSection(AccessTokenOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<AccessTokenOptions>, AccessTokenOptionsValidator>();

        services.AddSingleton<IAccessTokenProvider, JwtAccessTokenProvider>();

        services
            .AddOptions<RefreshTokenOptions>()
            .Bind(configuration.GetSection(RefreshTokenOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<RefreshTokenOptions>, RefreshTokenOptionsValidator>();

        services.AddSingleton<IRefreshTokenGenerator, SecureRefreshTokenGenerator>();

        services.AddSingleton<IRefreshTokenHasher, Sha256RefreshTokenHasher>();

        return services;
    }
}
