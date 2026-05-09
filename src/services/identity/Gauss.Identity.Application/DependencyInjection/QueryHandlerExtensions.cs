using Gauss.BuildingBlocks.Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Gauss.Identity.Application.DependencyInjection;

public static class QueryHandlerExtensions
{
    public static IServiceCollection AddQueryHandler<TQuery, TResponse, THandler>(
        this IServiceCollection services)
        where TQuery : IQuery<TResponse>
        where THandler : class, IQueryHandler<TQuery, TResponse>
    {
        services.AddScoped<THandler>();

        services.AddScoped<IQueryHandler<TQuery, TResponse>>(
            serviceProvider => serviceProvider.GetRequiredService<THandler>());

        return services;
    }
}
