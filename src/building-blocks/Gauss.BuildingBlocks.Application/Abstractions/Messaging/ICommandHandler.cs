using Gauss.BuildingBlocks.Application.Abstractions.Results;

namespace Gauss.BuildingBlocks.Application.Abstractions.Messaging;

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task<Result> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default);
}
