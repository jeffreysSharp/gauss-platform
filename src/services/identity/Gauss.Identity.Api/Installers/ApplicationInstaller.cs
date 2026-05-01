using FluentValidation;
using Gauss.BuildingBlocks.Application.Abstractions.Messaging;
using Gauss.Identity.Application.Users.RegisterUser;

namespace Gauss.Identity.Api.Installers;

public sealed class ApplicationInstaller : IInstaller
{
    public void InstallServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<
            ICommandHandler<RegisterUserCommand, RegisterUserResponse>,
            RegisterUserCommandHandler>();

        services.AddScoped<
            IValidator<RegisterUserCommand>,
            RegisterUserCommandValidator>();
    }
}
