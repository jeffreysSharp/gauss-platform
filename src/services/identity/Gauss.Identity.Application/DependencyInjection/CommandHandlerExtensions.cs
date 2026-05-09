using FluentValidation;
using Gauss.BuildingBlocks.Application.Abstractions.Messaging;
using Gauss.BuildingBlocks.Application.Behaviors.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Gauss.Identity.Application.DependencyInjection;

public static class CommandHandlerExtensions
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
