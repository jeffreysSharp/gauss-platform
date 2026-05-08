using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Application.Abstractions.Tenancy;
using Gauss.Identity.Application.Abstractions.Time;
using Gauss.Identity.Infrastructure.Authentication;
using Gauss.Identity.Infrastructure.Persistence;
using Gauss.Identity.Infrastructure.Tenancy;
using Gauss.Identity.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Gauss.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddTenancy();
        services.AddAuthenticationServices(configuration);
        services.AddRefreshTokenServices(configuration);
        services.AddRedis(configuration);
        services.AddTimeProvider();

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<IdentityPersistenceOptions>(
            configuration.GetSection(IdentityPersistenceOptions.SectionName));

        services.AddSingleton<IdentityDbConnectionFactory>();

        services.AddScoped<IUserRepository, SqlUserRepository>();
        services.AddScoped<IPermissionRepository, SqlPermissionRepository>();
        services.AddScoped<IRoleRepository, SqlRoleRepository>();

        return services;
    }

    private static IServiceCollection AddTenancy(
        this IServiceCollection services)
    {
        services.AddScoped<ITenantProvisioningService, SqlTenantProvisioningService>();

        return services;
    }

    private static IServiceCollection AddAuthenticationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IPasswordHasher, AspNetCorePasswordHasher>();

        services
            .AddOptions<AccessTokenOptions>()
            .Bind(configuration.GetSection(AccessTokenOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<AccessTokenOptions>, AccessTokenOptionsValidator>();

        services.AddSingleton<IAccessTokenProvider, JwtAccessTokenProvider>();

        return services;
    }

    private static IServiceCollection AddRefreshTokenServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<RefreshTokenOptions>()
            .Bind(configuration.GetSection(RefreshTokenOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<RefreshTokenOptions>, RefreshTokenOptionsValidator>();

        services.AddSingleton<IRefreshTokenGenerator, SecureRefreshTokenGenerator>();
        services.AddSingleton<IRefreshTokenHasher, Sha256RefreshTokenHasher>();

        services.AddScoped<IRefreshTokenStore, RedisRefreshTokenStore>();

        return services;
    }

    private static IServiceCollection AddRedis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<RedisOptions>()
            .Bind(configuration.GetSection(RedisOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<RedisOptions>, RedisOptionsValidator>();

        services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<RedisOptions>>()
                .Value;

            return ConnectionMultiplexer.Connect(options.ConnectionString);
        });

        return services;
    }

    private static IServiceCollection AddTimeProvider(
        this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }
}
