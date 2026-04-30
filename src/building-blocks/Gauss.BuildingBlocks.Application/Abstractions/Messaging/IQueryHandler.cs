using Gauss.BuildingBlocks.Application.Abstractions.Results;

namespace Gauss.BuildingBlocks.Application.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken = default);
}
