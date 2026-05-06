using FluentValidation;
using Gauss.BuildingBlocks.Application.Abstractions.Messaging;
using Gauss.BuildingBlocks.Application.Behaviors.Validation;
using Gauss.Identity.Application.Authentication.Login;
using Gauss.Identity.Application.Authentication.RefreshTokens;
using Gauss.Identity.Application.Users.RegisterUser;

namespace Gauss.Identity.Api.Installers;

public sealed class ApplicationInstaller : IInstaller
{
    public void InstallServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<RegisterUserCommandHandler>();
        services.AddScoped<LoginCommandHandler>();

        services.AddScoped<IValidator<RegisterUserCommand>, RegisterUserCommandValidator>();
        services.AddScoped<IValidator<LoginCommand>, LoginCommandValidator>();

        services.AddScoped<ICommandHandler<RegisterUserCommand, RegisterUserResponse>>(serviceProvider =>
        {
            var innerHandler = serviceProvider.GetRequiredService<RegisterUserCommandHandler>();

            var validators = serviceProvider.GetServices<IValidator<RegisterUserCommand>>();

            return new ValidationCommandHandlerDecorator<RegisterUserCommand, RegisterUserResponse>(
                innerHandler,
                validators);
        });

        services.AddScoped<ICommandHandler<LoginCommand, LoginResponse>>(serviceProvider =>
        {
            var innerHandler = serviceProvider.GetRequiredService<LoginCommandHandler>();

            var validators = serviceProvider.GetServices<IValidator<LoginCommand>>();

            return new ValidationCommandHandlerDecorator<LoginCommand, LoginResponse>(
                innerHandler,
                validators);
        });

        services.AddScoped<RefreshTokenCommandHandler>();

        services.AddScoped<IValidator<RefreshTokenCommand>, RefreshTokenCommandValidator>();

        services.AddScoped<ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>>(serviceProvider =>
        {
            var innerHandler = serviceProvider.GetRequiredService<RefreshTokenCommandHandler>();

            var validators = serviceProvider.GetServices<IValidator<RefreshTokenCommand>>();

            return new ValidationCommandHandlerDecorator<RefreshTokenCommand, RefreshTokenResponse>(
                innerHandler,
                validators);
        });
    }
}
