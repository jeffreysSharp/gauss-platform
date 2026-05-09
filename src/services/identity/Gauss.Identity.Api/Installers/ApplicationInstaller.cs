using FluentValidation;
using Gauss.BuildingBlocks.Application.Abstractions.Messaging;
using Gauss.BuildingBlocks.Application.Abstractions.Results;
using Gauss.BuildingBlocks.Application.Behaviors.Validation;
using Gauss.Identity.Application.Abstractions.Authorization;
using Gauss.Identity.Application.Authentication.Login;
using Gauss.Identity.Application.Authentication.RefreshTokens;
using Gauss.Identity.Application.Authorization;
using Gauss.Identity.Application.Authorization.GetPermissions;
using Gauss.Identity.Application.Users.RegisterUser;

namespace Gauss.Identity.Api.Installers;

public sealed class ApplicationInstaller : IInstaller
{
    public int Order => InstallerOrder.Application;

    public void InstallServices(
        IServiceCollection services,
        IConfiguration configuration)
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
    }
}

internal static class ApplicationInstallerExtensions
{
    public static IServiceCollection AddCommandHandlerWithValidation<TCommand, TResponse, THandler>(
        this IServiceCollection services)
        where TCommand : ICommand<TResponse>
        where THandler : class, ICommandHandler<TCommand, TResponse>
    {
        services.AddScoped<THandler>();

        services.AddScoped<ICommandHandler<TCommand, TResponse>>(serviceProvider =>
        {
            var innerHandler = serviceProvider.GetRequiredService<THandler>();

            var validators = serviceProvider.GetServices<IValidator<TCommand>>();

            return new ValidationCommandHandlerDecorator<TCommand, TResponse>(
                innerHandler,
                validators);
        });

        return services;
    }
}
