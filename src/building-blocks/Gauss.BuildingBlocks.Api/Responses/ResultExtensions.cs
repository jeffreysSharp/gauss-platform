using Gauss.BuildingBlocks.Application.Abstractions.Results;
using Microsoft.AspNetCore.Http;

namespace Gauss.BuildingBlocks.Api.Responses;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsSuccess
            ? Results.NoContent()
            : result.Error.ToProblemResult();
    }

    public static IResult ToHttpResult<TValue>(
        this Result<TValue> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.Error.ToProblemResult();
    }

    public static IResult ToHttpResult<TValue>(
        this Result<TValue> result,
        Func<TValue, IResult> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);

        return result.IsSuccess
            ? onSuccess(result.Value)
            : result.Error.ToProblemResult();
    }

    public static IResult ToProblemResult(this Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        var problemDetails = ProblemDetailsMapper.ToProblemDetails(error);

        return Results.Problem(
            detail: problemDetails.Detail,
            statusCode: problemDetails.Status,
            title: problemDetails.Title,
            type: problemDetails.Type,
            extensions: problemDetails.Extensions);
    }
}
