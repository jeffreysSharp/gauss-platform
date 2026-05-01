using FluentValidation;
using Gauss.BuildingBlocks.Application.Abstractions.Messaging;
using Gauss.BuildingBlocks.Application.Abstractions.Results;

namespace Gauss.BuildingBlocks.Application.Behaviors.Validation;

public sealed class ValidationCommandHandlerDecorator<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> innerHandler,
    IEnumerable<IValidator<TCommand>> validators)
    : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async Task<Result<TResponse>> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validatorsArray = validators.ToArray();

        if (validatorsArray.Length == 0)
        {
            return await innerHandler.HandleAsync(
                command,
                cancellationToken);
        }

        var validationContext = new ValidationContext<TCommand>(command);

        var validationResults = await Task.WhenAll(
            validatorsArray.Select(validator =>
                validator.ValidateAsync(validationContext, cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToArray();

        if (failures.Length == 0)
        {
            return await innerHandler.HandleAsync(
                command,
                cancellationToken);
        }

        var firstFailure = failures[0];

        var errorCode = string.IsNullOrWhiteSpace(firstFailure.ErrorCode)
            ? "Validation.Failed"
            : firstFailure.ErrorCode;

        var errorDescription = string.IsNullOrWhiteSpace(firstFailure.ErrorMessage)
            ? "A validation error occurred."
            : firstFailure.ErrorMessage;

        return Result<TResponse>.Failure(
            Error.Validation(
                errorCode,
                errorDescription));
    }
}
