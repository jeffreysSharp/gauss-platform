namespace Gauss.BuildingBlocks.Application.Abstractions.Results;

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(TValue value)
        : base(true, Error.None)
    {
        _value = value;
    }

    private Result(Error error)
        : base(false, error)
    {
        _value = default;
    }

    public TValue Value
    {
        get
        {
            if (IsFailure)
            {
                throw new InvalidOperationException("The value of a failed result cannot be accessed.");
            }

            return _value!;
        }
    }

    public static Result<TValue> Success(TValue value)
    {
        return new Result<TValue>(value);
    }

    public static new Result<TValue> Failure(Error error)
    {
        return new Result<TValue>(error);
    }
}
