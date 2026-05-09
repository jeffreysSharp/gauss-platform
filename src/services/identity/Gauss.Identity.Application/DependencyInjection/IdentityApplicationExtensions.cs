using FluentValidation;
using Gauss.BuildingBlocks.Application.Abstractions.Messaging;
using Gauss.Identity.Application.Abstractions.Authorization;
using Gauss.Identity.Application.Authentication.Login;
using Gauss.Identity.Application.Authentication.RefreshTokens;
using Gauss.Identity.Application.Authorization;
using Gauss.Identity.Application.Authorization.GetPermissions;
using Gauss.Identity.Application.Users.RegisterUser;
using Microsoft.Extensions.DependencyInjection;

namespace Gauss.Identity.Application.DependencyInjection;

public static class IdentityApplicationExtensions
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddCommandHandlerWithValidation<
            RegisterUserCommand,
            RegisterUserResponse,
            RegisterUserCommandHandler>();

        services.AddCommandHandlerWithValidation<
            LoginCommand,
            LoginResponse,
            LoginCommandHandler>();

        services.AddCommandHandlerWithValidation<
            RefreshTokenCommand,
            RefreshTokenResponse,
            RefreshTokenCommandHandler>();

        services.AddScoped<IValidator<RegisterUserCommand>, RegisterUserCommandValidator>();
        services.AddScoped<IValidator<LoginCommand>, LoginCommandValidator>();
        services.AddScoped<IValidator<RefreshTokenCommand>, RefreshTokenCommandValidator>();

        services.AddScoped<IPermissionAuthorizationService, PermissionAuthorizationService>();

        services.AddScoped<GetPermissionsQueryHandler>();

        services.AddScoped<IQueryHandler<GetPermissionsQuery, IReadOnlyCollection<GetPermissionResponse>>>(
            serviceProvider => serviceProvider.GetRequiredService<GetPermissionsQueryHandler>());

        return services;
    }
}
