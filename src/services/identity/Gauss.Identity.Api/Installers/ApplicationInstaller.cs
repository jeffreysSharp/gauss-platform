using FluentValidation;
using Gauss.BuildingBlocks.Application.Abstractions.Messaging;
using Gauss.BuildingBlocks.Application.Behaviors.Validation;
using Gauss.Identity.Application.Users.RegisterUser;

namespace Gauss.Identity.Api.Installers;

public sealed class ApplicationInstaller : IInstaller
{
    public void InstallServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<RegisterUserCommandHandler>();

        services.AddScoped<IValidator<RegisterUserCommand>, RegisterUserCommandValidator>();

        services.AddScoped<ICommandHandler<RegisterUserCommand, RegisterUserResponse>>(serviceProvider =>
        {
            var innerHandler = serviceProvider.GetRequiredService<RegisterUserCommandHandler>();

            var validators = serviceProvider.GetServices<IValidator<RegisterUserCommand>>();

            return new ValidationCommandHandlerDecorator<RegisterUserCommand, RegisterUserResponse>(
                innerHandler,
                validators);
        });
    }
}
